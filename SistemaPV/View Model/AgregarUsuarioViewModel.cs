using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SistemaPV.Model;
using SistemaPV.Repositories;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls; // Para PasswordBox

namespace SistemaPV.View_Model 
{
    public class AgregarUsuarioViewModel : ViewModelBase 
    {
        private readonly UsuarioRepository _usuarioRepo;
        private readonly RolRepository _rolRepo;

        // Propiedades para la Vista
        private string _nombre;
        public string Nombre
        {
            get => _nombre;
            set { _nombre = value; OnPropertyChanged(nameof(Nombre)); } 
        }

        private string _aPaterno;
        public string APaterno
        {
            get => _aPaterno;
            set { _aPaterno = value; OnPropertyChanged(nameof(APaterno)); }
        }

        private string _aMaterno;
        public string AMaterno
        {
            get => _aMaterno;
            set { _aMaterno = value; OnPropertyChanged(nameof(AMaterno)); }
        }

        private string _nombreUsuario;
        public string NombreUsuario
        {
            get => _nombreUsuario;
            set { _nombreUsuario = value; OnPropertyChanged(nameof(NombreUsuario)); }
        }

        private ObservableCollection<Rol> _roles;
        public ObservableCollection<Rol> Roles
        {
            get => _roles;
            set { _roles = value; OnPropertyChanged(nameof(Roles)); }
        }

        private Rol _selectedRol;
        public Rol SelectedRol
        {
            get => _selectedRol;
            set { _selectedRol = value; OnPropertyChanged(nameof(SelectedRol)); }
        }

        private string _titulo;
        public string Titulo
        {
            get => _titulo;
            set { _titulo = value; OnPropertyChanged(nameof(Titulo)); }
        }

        private string _botonGuardarTexto;
        public string BotonGuardarTexto
        {
            get => _botonGuardarTexto;
            set { _botonGuardarTexto = value; OnPropertyChanged(nameof(BotonGuardarTexto)); }
        }

        
        private bool _isNombreUsuarioEnabled = true;
        public bool IsNombreUsuarioEnabled
        {
            get => _isNombreUsuarioEnabled;
            set { _isNombreUsuarioEnabled = value; OnPropertyChanged(nameof(IsNombreUsuarioEnabled)); }
        }

        private Visibility _passwordFieldsVisibility = Visibility.Visible;
        public Visibility PasswordFieldsVisibility
        {
            get => _passwordFieldsVisibility;
            set { _passwordFieldsVisibility = value; OnPropertyChanged(nameof(PasswordFieldsVisibility)); }
        }

        private GridLength _rowPasswordHeight = new GridLength(1, GridUnitType.Auto);
        public GridLength RowPasswordHeight
        {
            get => _rowPasswordHeight;
            set { _rowPasswordHeight = value; OnPropertyChanged(nameof(RowPasswordHeight)); }
        }

        private GridLength _rowPasswordConfirmHeight = new GridLength(1, GridUnitType.Auto);
        public GridLength RowPasswordConfirmHeight
        {
            get => _rowPasswordConfirmHeight;
            set { _rowPasswordConfirmHeight = value; OnPropertyChanged(nameof(RowPasswordConfirmHeight)); }
        }

        // Estado
        private bool _isEditMode;
        private int? _usuarioId;

        // Evento de cierre
        public event Action<bool?> RequestClose;

        // Comandos
        public ViewModelCommand GuardarCommand { get; private set; } 
        public ViewModelCommand CancelarCommand { get; private set; }
        public ViewModelCommand WindowLoadedCommand { get; private set; }

        // Constructor para "Agregar"
        public AgregarUsuarioViewModel()
        {
            _usuarioRepo = new UsuarioRepository();
            _rolRepo = new RolRepository();

            Titulo = "Agregar Nuevo Usuario";
            BotonGuardarTexto = "Guardar";
            _isEditMode = false;

            GuardarCommand = new ViewModelCommand(OnGuardar);
            CancelarCommand = new ViewModelCommand(OnCancelar);
            WindowLoadedCommand = new ViewModelCommand(OnWindowLoaded);
        }

        // Constructor para "Editar"
        public AgregarUsuarioViewModel(Usuario usuarioAEditar) : this()
        {
            _isEditMode = true;
            _usuarioId = usuarioAEditar.ID_USUARIO;

            Nombre = usuarioAEditar.NOMBRE;
            APaterno = usuarioAEditar.APATERNO;
            AMaterno = usuarioAEditar.AMATERNO;
            NombreUsuario = usuarioAEditar.NOMBRE_USUARIO;

            
            Titulo = "Editar Usuario";
            BotonGuardarTexto = "Actualizar";
            IsNombreUsuarioEnabled = false;
            PasswordFieldsVisibility = Visibility.Collapsed;
            RowPasswordHeight = new GridLength(0);
            RowPasswordConfirmHeight = new GridLength(0);

            
        }

        private void OnWindowLoaded(object obj)
        {
            try
            {
                var rolesList = _rolRepo.GetRoles();
                Roles = new ObservableCollection<Rol>(rolesList);

                if (_isEditMode)
                {
                    // Estamos en modo Editar, buscamos el rol del usuario
                    SelectedRol = Roles.FirstOrDefault(r => r.ID_ROL == _usuarioId.Value);
                }
                else if (Roles.Count > 0)
                {
                    // Modo Agregar, seleccionamos el primero (tu lógica original)
                    SelectedRol = Roles[0];
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar los roles: " + ex.Message);
                RequestClose?.Invoke(false);
            }
        }

        private void OnGuardar(object parameter)
        {
            // 1. Obtener contraseñas
            string password = "";
            string passwordConfirm = "";
            if (!_isEditMode)
            {
                var passwordBoxes = parameter as object[];
                if (passwordBoxes != null && passwordBoxes.Length == 2)
                {
                    password = (passwordBoxes[0] as PasswordBox).Password;
                    passwordConfirm = (passwordBoxes[1] as PasswordBox).Password;
                }
            }

            
            if (string.IsNullOrEmpty(Nombre) || string.IsNullOrEmpty(APaterno) || string.IsNullOrEmpty(NombreUsuario))
            {
                MessageBox.Show("Nombre, Apellido Paterno y Nombre de Usuario son obligatorios.", "Campos vacíos", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (Nombre.Length > 50) { MessageBox.Show("El nombre no puede tener más de 50 caracteres.", "Error de longitud", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
            if (APaterno.Length > 50) { MessageBox.Show("El apellido paterno no puede tener más de 50 caracteres.", "Error de longitud", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
            if (!string.IsNullOrEmpty(AMaterno) && AMaterno.Length > 50) { MessageBox.Show("El apellido materno no puede tener más de 50 caracteres.", "Error de longitud", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
            if (NombreUsuario.Length > 50) { MessageBox.Show("El nombre de usuario no puede tener más de 50 caracteres.", "Error de longitud", MessageBoxButton.OK, MessageBoxImage.Warning); return; }

            if (!Regex.IsMatch(NombreUsuario, @"^[a-zA-Z0-9._-]+$"))
            {
                MessageBox.Show("El nombre de usuario solo puede contener letras, números, puntos, guiones y underscores.", "Caracteres inválidos", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!_isEditMode) // Validaciones de contraseña solo en modo "Agregar"
            {
                if (string.IsNullOrEmpty(password))
                {
                    MessageBox.Show("La contraseña es obligatoria.", "Campos vacíos", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (password.Length > 50) { MessageBox.Show("La contraseña no puede tener más de 50 caracteres.", "Error de longitud", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
                if (password != passwordConfirm)
                {
                    MessageBox.Show("Las contraseñas no coinciden.", "Error de Contraseña", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            if (SelectedRol == null)
            {
                MessageBox.Show("Debe seleccionar un rol para el usuario.", "Error de Rol", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            // --- FIN DE LÓGICA DE VALIDACIÓN ---

            var usuario = new Usuario
            {
                NOMBRE = this.Nombre,
                APATERNO = this.APaterno,
                AMATERNO = string.IsNullOrEmpty(this.AMaterno) ? null : this.AMaterno,
                NOMBRE_USUARIO = this.NombreUsuario,
                ID_ROL = this.SelectedRol.ID_ROL
            };

            try
            {
                bool success;
                string successMessage;

                if (_isEditMode)
                {
                    usuario.ID_USUARIO = _usuarioId.Value;
                    success = _usuarioRepo.UpdateUsuario(usuario);
                    successMessage = "¡Usuario actualizado exitosamente!";
                }
                else
                {
                    success = _usuarioRepo.AddUsuario(usuario, password);
                    successMessage = "¡Usuario creado exitosamente!";
                }

                if (success)
                {
                    MessageBox.Show(successMessage, "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    RequestClose?.Invoke(true); // Cierra la ventana
                }
            }
            catch (SqlException sqlEx) 
            {
                if (sqlEx.Number == 2627 || sqlEx.Number == 2601)
                {
                    MessageBox.Show("Error: El nombre de usuario '" + usuario.NOMBRE_USUARIO + "' ya existe.", "Error de Duplicado", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    MessageBox.Show("Error de base de datos: " + sqlEx.Message, "Error SQL", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error inesperado: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnCancelar(object obj)
        {
            RequestClose?.Invoke(false); // Cierra la ventana
        }
    }
}