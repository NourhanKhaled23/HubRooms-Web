using System.Collections.ObjectModel;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using Microsoft.AspNetCore.SignalR.Client;

namespace AdvancedChat.Desktop;

public partial class MainWindow : Window
{
    private readonly CookieContainer _cookies = new();
    private readonly ObservableCollection<RoomDto> _rooms = [];
    private readonly ObservableCollection<ChatEnvelope> _messages = [];
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

    private HttpClient? _httpClient;
    private HubConnection? _hubConnection;
    private RoomDto? _selectedRoom;
    private bool _isAuthenticated;

    public MainWindow()
    {
        InitializeComponent();
        RoomsList.ItemsSource = _rooms;
        MessagesList.ItemsSource = _messages;
        UpdateConnectionState(false);
    }

    private async void Register_Click(object sender, RoutedEventArgs e)
    {
        await AuthenticateAsync("api/auth/register");
    }

    private async void Login_Click(object sender, RoutedEventArgs e)
    {
        await AuthenticateAsync("api/auth/login");
    }

    private async Task AuthenticateAsync(string endpoint)
    {
        var email = EmailBox.Text.Trim();
        var password = PasswordBox.Password;

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            SetStatus("Enter email and password.", "warning");
            return;
        }

        SetButtonsEnabled(false);

        try
        {
            EnsureHttpClient();
            var response = await _httpClient!.PostAsJsonAsync(endpoint, new AuthRequest(email, password), _jsonOptions);

            if (!response.IsSuccessStatusCode)
            {
                await ShowServerErrorAsync(response);
                return;
            }

            _isAuthenticated = true;
            SetStatus($"Signed in as {email}", "success");
            await ConnectHubAsync();
            await LoadRoomsAsync();
        }
        catch (Exception ex)
        {
            SetStatus($"Connection failed: {ex.Message}", "error");
        }
        finally
        {
            SetButtonsEnabled(true);
        }
    }

    private async Task ConnectHubAsync()
    {
        if (_hubConnection is not null)
        {
            await _hubConnection.DisposeAsync();
        }

        _hubConnection = new HubConnectionBuilder()
            .WithUrl($"{GetServerUrl()}/chatHub", options => options.Cookies = _cookies)
            .WithAutomaticReconnect()
            .Build();

        _hubConnection.Reconnecting += _ =>
        {
            Dispatcher.Invoke(() => UpdateConnectionState(false, "Reconnecting..."));
            return Task.CompletedTask;
        };

        _hubConnection.Reconnected += _ =>
        {
            Dispatcher.Invoke(() =>
            {
                UpdateConnectionState(true);
                SetStatus("Reconnected", "success");
                if (_selectedRoom is not null)
                    _hubConnection!.InvokeAsync("JoinRoom", _selectedRoom.Id);
            });
            return Task.CompletedTask;
        };

        _hubConnection.Closed += _ =>
        {
            Dispatcher.Invoke(() =>
            {
                UpdateConnectionState(false);
                _isAuthenticated = false;
                SetStatus("Disconnected", "error");
            });
            return Task.CompletedTask;
        };

        _hubConnection.On<ChatEnvelope>("ReceiveMessage", message =>
        {
            Dispatcher.Invoke(() =>
            {
                if (_selectedRoom?.Id == message.RoomId)
                {
                    _messages.Add(message);
                    Dispatcher.Invoke(() => ScrollToBottom());
                }
            });
        });

        try
        {
            await _hubConnection.StartAsync();
            UpdateConnectionState(true);
        }
        catch
        {
            UpdateConnectionState(false);
            throw;
        }
    }

    private async void RefreshRooms_Click(object sender, RoutedEventArgs e)
    {
        await LoadRoomsAsync();
    }

    private async Task LoadRoomsAsync()
    {
        if (!_isAuthenticated) return;

        try
        {
            EnsureHttpClient();
            var rooms = await _httpClient!.GetFromJsonAsync<List<RoomDto>>("api/rooms", _jsonOptions) ?? [];
            var previousId = _selectedRoom?.Id;
            _rooms.Clear();
            foreach (var room in rooms)
            {
                _rooms.Add(room);
            }

            if (previousId.HasValue)
            {
                var match = _rooms.FirstOrDefault(r => r.Id == previousId.Value);
                if (match is not null) RoomsList.SelectedItem = match;
            }

            if (_rooms.Count == 0)
            {
                SetStatus("No rooms yet. Create one!", "info");
            }
        }
        catch (Exception ex)
        {
            SetStatus($"Failed to load rooms: {ex.Message}", "error");
        }
    }

    private async void CreateRoom_Click(object sender, RoutedEventArgs e)
    {
        var name = NewRoomNameBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            SetStatus("Enter a room name.", "warning");
            return;
        }

        CreateRoomBtn.IsEnabled = false;
        CreateRoomBtn.Content = "Creating...";

        try
        {
            EnsureHttpClient();
            var response = await _httpClient!.PostAsJsonAsync("api/rooms", new
            {
                name,
                description = NewRoomDescriptionBox.Text.Trim()
            }, _jsonOptions);

            if (!response.IsSuccessStatusCode)
            {
                await ShowServerErrorAsync(response);
                return;
            }

            NewRoomNameBox.Clear();
            NewRoomDescriptionBox.Clear();
            await LoadRoomsAsync();
            SetStatus("Room created.", "success");
        }
        catch (Exception ex)
        {
            SetStatus($"Failed: {ex.Message}", "error");
        }
        finally
        {
            CreateRoomBtn.Content = "\u2795  Create Room";
            CreateRoomBtn.IsEnabled = true;
        }
    }

    private async void AddUser_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedRoom is null)
        {
            SetStatus("Select a room first.", "warning");
            return;
        }

        var email = InviteEmailBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(email))
        {
            SetStatus("Enter an email address.", "warning");
            return;
        }

        InviteBtn.IsEnabled = false;

        try
        {
            EnsureHttpClient();
            var response = await _httpClient!.PostAsJsonAsync($"api/rooms/{_selectedRoom.Id}/users", new
            {
                email
            }, _jsonOptions);

            if (!response.IsSuccessStatusCode)
            {
                await ShowServerErrorAsync(response);
                return;
            }

            InviteEmailBox.Clear();
            SetStatus($"Invited {email} to room.", "success");
        }
        catch (Exception ex)
        {
            SetStatus($"Failed: {ex.Message}", "error");
        }
        finally
        {
            InviteBtn.IsEnabled = true;
        }
    }

    private async void RoomsList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        _selectedRoom = RoomsList.SelectedItem as RoomDto;
        _messages.Clear();

        if (_selectedRoom is null || _hubConnection is null || !_isAuthenticated)
        {
            RoomTitleText.Text = "Select a room";
            RoomDescriptionText.Text = string.Empty;
            MessageBox.IsEnabled = false;
            SendBtn.IsEnabled = false;
            return;
        }

        RoomTitleText.Text = _selectedRoom.Name;
        RoomDescriptionText.Text = _selectedRoom.Description ?? "No description";
        MessageBox.IsEnabled = true;
        SendBtn.IsEnabled = true;

        try
        {
            await _hubConnection.InvokeAsync("JoinRoom", _selectedRoom.Id);
            var recent = await _hubConnection.InvokeAsync<List<ChatEnvelope>>("GetRecentMessages", _selectedRoom.Id);
            foreach (var message in recent)
            {
                _messages.Add(message);
            }
            Dispatcher.Invoke(() => ScrollToBottom());
        }
        catch (Exception ex)
        {
            SetStatus($"Failed to load messages: {ex.Message}", "error");
        }
    }

    private async void Send_Click(object sender, RoutedEventArgs e)
    {
        await SendMessageAsync();
    }

    private async void MessageBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && !e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Shift))
        {
            e.Handled = true;
            await SendMessageAsync();
        }
    }

    private async Task SendMessageAsync()
    {
        if (_selectedRoom is null || _hubConnection is null)
        {
            SetStatus("Select a room first.", "warning");
            return;
        }

        var text = MessageBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(text)) return;

        try
        {
            await _hubConnection.InvokeAsync("SendMessage", _selectedRoom.Id, text);
            MessageBox.Clear();
            MessageBox.Focus();
        }
        catch (Exception ex)
        {
            SetStatus($"Send failed: {ex.Message}", "error");
        }
    }

    private void EnsureHttpClient()
    {
        var handler = new HttpClientHandler
        {
            CookieContainer = _cookies,
            UseCookies = true
        };

        _httpClient?.Dispose();
        _httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri(GetServerUrl())
        };
    }

    private string GetServerUrl()
    {
        return ServerUrlBox.Text.Trim().TrimEnd('/');
    }

    private async Task ShowServerErrorAsync(HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync();
        SetStatus($"{(int)response.StatusCode}: {body}", "error");
    }

    private void SetStatus(string message, string type = "info")
    {
        StatusText.Text = message;
        StatusBadge.Background = type switch
        {
            "success" => new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0xDC, 0xFC, 0xE7)),
            "warning" => new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0xFE, 0xF3, 0xC7)),
            "error" => new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0xFE, 0xE2, 0xE2)),
            _ => new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0xF1, 0xF5, 0xF9))
        };
        StatusText.Foreground = type switch
        {
            "success" => new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x16, 0x65, 0x34)),
            "warning" => new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x92, 0x40, 0x0E)),
            "error" => new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x99, 0x1B, 0x1B)),
            _ => new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x47, 0x55, 0x69))
        };
    }

    private void UpdateConnectionState(bool connected, string text = "")
    {
        if (connected)
        {
            StatusDot.Fill = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x16, 0xA3, 0x4A));
            ConnectionStatusDot.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0xDC, 0xFC, 0xE7));
            ConnectionStatusText.Text = "Connected";
            ConnectionStatusText.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x16, 0x65, 0x34));
        }
        else
        {
            StatusDot.Fill = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0xEF, 0x44, 0x44));
            ConnectionStatusDot.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0xFE, 0xE2, 0xE2));
            ConnectionStatusText.Text = string.IsNullOrEmpty(text) ? "Offline" : text;
            ConnectionStatusText.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x99, 0x1B, 0x1B));
        }
    }

    private void SetButtonsEnabled(bool enabled)
    {
        RegisterBtn.IsEnabled = enabled;
        LoginBtn.IsEnabled = enabled;
    }

    private void ScrollToBottom()
    {
        if (MessagesScroll.ViewportHeight < MessagesScroll.ExtentHeight)
            MessagesScroll.ScrollToBottom();
    }
}

public record AuthRequest(string Email, string Password);

public record RoomDto(int Id, string Name, string? Description, int MemberCount);

public record ChatEnvelope(int RoomId, string UserName, string Text, DateTime SentAt);
