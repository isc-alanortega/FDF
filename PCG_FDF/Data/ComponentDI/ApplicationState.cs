using Blazored.LocalStorage;
using Microsoft.JSInterop;
using PCG_ENTITIES.PCG_FDF.UtilityEntities;

namespace PCG_FDF.Data.ComponentDI
{
	public sealed class ApplicationState
	{
		private int Current_Location_ID { get; set; }
		private int? Previous_Location_ID { get; set; }
		private IDictionary<int, KeyValue> Available_Locations { get; set; } = new Dictionary<int, KeyValue>();
		private IDictionary<int, ClienteVinculadoEntidad> Available_Subclients { get; set; } = new Dictionary<int, ClienteVinculadoEntidad>();
		private HashSet<string>? Permissions { get; set; }
		private int? Principal_Client_ID { get; set; }
		private int? Current_Client_ID { get; set; }
		private int? Previous_Client_ID { get; set; }
		private bool User_Authenticated { get; set; } = false;
		private bool Can_Change_Location { get; set; } = true;
		private bool Initialized { get; set; } = false;
		private string? Intecon_User { get; set; }
		private string? Intecon_Password { get; set; }
		private bool? Intecon_Authenticated { get; set; }

		private readonly ILocalStorageService _localStorage;
		private readonly IJSRuntime JS;

		/// <summary>
		/// Action that triggers a StateHasChanged event to rerender the shared components
		/// </summary>
		public event Action OnChange;

		public ApplicationState(ILocalStorageService localStorage, IJSRuntime js)
		{
			_localStorage = localStorage;
			JS = js;
		}

		/// <summary>
		/// Triggers the StateHasChanged event on shared components to rerender themselves
		/// </summary>
		private void NotifyStateChanged() => OnChange?.Invoke();

		public async Task LogAsync(string message)
		{
			await JS.InvokeVoidAsync("console.warn", message);
		}

		public async Task LogAsync(object message)
		{
			await JS.InvokeVoidAsync("console.warn", message);
		}

		public void SetInteconUser(string? intecon_user)
		{
			Intecon_User = intecon_user;
		}

		public string? GetInteconUser()
		{
			return Intecon_User;
		}

		public void SetInteconPassword(string? intecon_password)
		{
			Intecon_Password = intecon_password;
		}

		public string? GetInteconPassword()
		{
			return Intecon_Password;
		}

		public void SetInteconAuthenticated(bool? intecon_authenticated)
		{
			Intecon_Authenticated = intecon_authenticated;
			NotifyStateChanged();
		}

		public bool? GetInteconAuthenticated()
		{
			return Intecon_Authenticated;
		}

		public void SetPrincipalClient(int principal_client_id)
		{
			Principal_Client_ID = principal_client_id;
		}

		public int? GetPrincipalClient()
		{
			return Principal_Client_ID;
		}

		public bool GetUserAuthenticated()
		{
			return User_Authenticated;
		}

		public async Task SetUserAuthenticated(bool userAuthenticated)
		{
			User_Authenticated = userAuthenticated;
			await TrySetup();
			// Si el ID del cliente actual no existe en la lista de subclientes disponibles (posiblemente por una sesión anterior)
			// entonces se selecciona el primer subcliente disponible como predeterminado
			if (Current_Client_ID.HasValue && (Available_Subclients is null || !Available_Subclients.ContainsKey(Current_Client_ID.Value)))
			{
				Current_Client_ID = Available_Subclients?.Keys.FirstOrDefault();
				await _localStorage.SetItemAsync("SelectedSubclient", Current_Client_ID);
			}

			NotifyStateChanged();
		}

		public void SetPermissions(HashSet<string> permissions)
		{
			Permissions = permissions;
			NotifyStateChanged();
		}

		public bool HasPermission(string permission)
		{
			if (Permissions is null)
			{
				return false;
			}
			return Permissions.Contains(permission);
		}

		public async Task TrySetup()
		{
			if (User_Authenticated && !Initialized)
			{
				var locations = await _localStorage.GetItemAsync<IDictionary<int, KeyValue>>("UserLocations");
				if (locations is not null)
				{
					Available_Locations = locations;
				}

				var subclients = await _localStorage.GetItemAsync<IDictionary<int, ClienteVinculadoEntidad>>("UserSubclients");
				if (subclients is not null)
				{
					Available_Subclients = subclients;
				}

				var selected_subclient = await _localStorage.GetItemAsync<int?>("SelectedSubclient");
				if (selected_subclient != default && selected_subclient is not null)
				{
					Current_Client_ID = Available_Subclients.TryGetValue(selected_subclient.Value, out _) ? selected_subclient : Available_Subclients.Keys.FirstOrDefault();

				}
				else if (Available_Subclients != default && Available_Subclients is not null && Available_Subclients.Any())
				{
					Current_Client_ID = Available_Subclients.Keys.FirstOrDefault();
				}

				var selected_location = await _localStorage.GetItemAsync<int>("SelectedLocation");
				if (selected_location != default)
				{
					if (Current_Client_ID.HasValue && Available_Subclients is not null && Available_Subclients[Current_Client_ID.Value].Locations.Contains(selected_location))
					{
						Current_Location_ID = selected_location;
					}
					else if (Current_Client_ID.HasValue && Available_Subclients is not null)
					{
						await SetCurrentLocation(Available_Subclients[Current_Client_ID.Value].Locations.First());
					}
				}
				Initialized = true;
				NotifyStateChanged();
			}
		}

		public async Task SetLocations(IEnumerable<KeyValue> locations)
		{
			Available_Locations = locations.ToDictionary(key => key.Key, value => value);
			await _localStorage.SetItemAsync("UserLocations", Available_Locations);
			NotifyStateChanged();

		}

		public async Task SetClients(IDictionary<int, ClienteVinculadoEntidad> clients)
		{
			Available_Subclients = clients;
			await _localStorage.SetItemAsync("UserSubclients", Available_Subclients);
			NotifyStateChanged();

		}

		public async Task SetCurrentLocation(int? locationID)
		{
			if (locationID.HasValue && Can_Change_Location && Available_Locations.TryGetValue(locationID.Value, out _))
			{
				Current_Location_ID = locationID.Value;
				await _localStorage.SetItemAsync("SelectedLocation", Current_Location_ID);
				NotifyStateChanged();

			}
		}

		public async Task SetCurrentClient(int? Client_ID)
		{
			if (Client_ID.HasValue && Can_Change_Location && Available_Subclients.TryGetValue(Client_ID.Value, out _))
			{
				Current_Client_ID = Client_ID.Value;
				await _localStorage.SetItemAsync("SelectedSubclient", Current_Client_ID);
				NotifyStateChanged();

			}
		}

		public void SetPreviousLocation(int? locationID)
		{
			Previous_Location_ID = locationID;
		}

		public void SetPreviousClient(int? Client_ID)
		{
			Previous_Client_ID = Client_ID;
		}

		public void SetCanChange(bool canChange)
		{
			Can_Change_Location = canChange;
		}

		public int GetCurrentLocation()
		{
			return Current_Location_ID;
		}

		public int? GetCurrentClientID()
		{
			return Current_Client_ID;
		}

		public IEnumerable<KeyValue> GetCurrentClientLocations()
		{
			if (Current_Client_ID is not null &&
				Available_Subclients is not null &&
				Available_Locations is not null &&
				Available_Subclients.ContainsKey(Current_Client_ID.Value))
			{
				return Available_Locations.Values.Where(location =>
					Available_Subclients[Current_Client_ID.Value].Locations?.Contains(location.Key) ?? false);
			}

			return Enumerable.Empty<KeyValue>();
		}


		public int? GetPreviousLocation()
		{
			return Previous_Location_ID;
		}

		public int? GetPreviousClient()
		{
			return Previous_Client_ID;
		}

		public IDictionary<int, KeyValue> GetAvailable_Locations()
		{
			return Available_Locations;
		}

		public IDictionary<int, ClienteVinculadoEntidad> GetAvailable_Subclients()
		{
			return Available_Subclients;
		}

		public string ReturnCurrentLocationName()
		{
			if (Available_Locations.TryGetValue(Current_Location_ID, out var location))
			{
				return location.Value;
			}
			return string.Empty;
		}

		public bool GetShouldShowPrices()
		{
			if (Current_Client_ID is not null && Available_Subclients.TryGetValue(Current_Client_ID.Value, out var client))
			{
				if (HasPermission("Ver Tarifas FDF"))
				{
					return client.Ver_Precio;
				}
				return false;
			}
			return false;
		}

		public string ReturnCurrentClientName()
		{
			if (Current_Client_ID is not null && Available_Subclients.TryGetValue(Current_Client_ID.Value, out var client))
			{
				return client.Entity_Name;
			}
			return string.Empty;
		}

		public string ReturnCurrentClientRFC()
		{
			if (Current_Client_ID is not null && Available_Subclients.TryGetValue(Current_Client_ID.Value, out var client))
			{
				return client.Client_RFC;
			}
			return string.Empty;
		}

		public bool GetCanChange()
		{
			return Can_Change_Location;
		}

		// Limpia completamente el estado actual del usuario, eliminando los datos en memoria y en localStorage.
		// Esto es útil al cerrar sesión para evitar que queden datos residuales de otro usuario.
		public async Task Clear()
		{
			Current_Location_ID = 0;
			Previous_Location_ID = null;
			Available_Locations = new Dictionary<int, KeyValue>();
			Available_Subclients = new Dictionary<int, ClienteVinculadoEntidad>();
			Permissions = null;
			Principal_Client_ID = null;
			Current_Client_ID = null;
			Previous_Client_ID = null;
			User_Authenticated = false;
			Initialized = false;
			Intecon_User = null;
			Intecon_Password = null;
			Intecon_Authenticated = null;

			await _localStorage.RemoveItemAsync("UserLocations");
			await _localStorage.RemoveItemAsync("UserSubclients");
			await _localStorage.RemoveItemAsync("SelectedSubclient");
			await _localStorage.RemoveItemAsync("SelectedLocation");

			NotifyStateChanged();
		}


	}
}
