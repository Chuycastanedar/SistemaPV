using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SistemaPV.Model;
using SistemaPV.Repositories;
using SistemaPV.Services;
using System.Collections.ObjectModel;
using System.Windows;

namespace SistemaPV.View_Model 
{
    public class InventarioViewModel : ViewModelBase 
    {
        private readonly ProductoRepository _productoRepo;
        private readonly UsuarioRepository _usuarioRepo;
        private readonly IDialogService _dialogService;

        // --- Propiedades para la Vista ---
        private ObservableCollection<Producto> _productos;
        public ObservableCollection<Producto> Productos
        {
            get => _productos;
            set { _productos = value; OnPropertyChanged(nameof(Productos)); }
        } 

        private ObservableCollection<Usuario> _usuarios;
        public ObservableCollection<Usuario> Usuarios
        {
            get => _usuarios;
            set { _usuarios = value; OnPropertyChanged(nameof(Usuarios)); }
        }

        private Producto _selectedProducto;
        public Producto SelectedProducto
        {
            get => _selectedProducto;
            set { _selectedProducto = value; OnPropertyChanged(nameof(SelectedProducto)); }
        }

        private Usuario _selectedUsuario;
        public Usuario SelectedUsuario
        {
            get => _selectedUsuario;
            set { _selectedUsuario = value; OnPropertyChanged(nameof(SelectedUsuario)); }
        }

        
        private Visibility _inventarioVisibility = Visibility.Visible;
        public Visibility InventarioVisibility
        {
            get => _inventarioVisibility;
            set { _inventarioVisibility = value; OnPropertyChanged(nameof(InventarioVisibility)); }
        }

        private Visibility _usuariosVisibility = Visibility.Collapsed;
        public Visibility UsuariosVisibility
        {
            get => _usuariosVisibility;
            set { _usuariosVisibility = value; OnPropertyChanged(nameof(UsuariosVisibility)); }
        }

        private Visibility _panelBotonesProductosVisibility = Visibility.Visible;
        public Visibility PanelBotonesProductosVisibility
        {
            get => _panelBotonesProductosVisibility;
            set { _panelBotonesProductosVisibility = value; OnPropertyChanged(nameof(PanelBotonesProductosVisibility)); }
        }

        private Visibility _panelBotonesUsuariosVisibility = Visibility.Collapsed;
        public Visibility PanelBotonesUsuariosVisibility
        {
            get => _panelBotonesUsuariosVisibility;
            set { _panelBotonesUsuariosVisibility = value; OnPropertyChanged(nameof(PanelBotonesUsuariosVisibility)); }
        }

        public event Action RequestLogout;

        // --- Comandos ---
        public ViewModelCommand WindowLoadedCommand { get; private set; } 
        public ViewModelCommand ShowInventarioCommand { get; private set; }
        public ViewModelCommand ShowUsuariosCommand { get; private set; }
        public ViewModelCommand CerrarSesionCommand { get; private set; }
        public ViewModelCommand AgregarProductoCommand { get; private set; }
        public ViewModelCommand EliminarProductoCommand { get; private set; }
        public ViewModelCommand EditarProductoCommand { get; private set; }
        public ViewModelCommand AgregarUsuarioCommand { get; private set; }
        public ViewModelCommand EliminarUsuarioCommand { get; private set; }
        public ViewModelCommand EditarUsuarioCommand { get; private set; }

        public InventarioViewModel(IDialogService dialogService)
        {
            _productoRepo = new ProductoRepository();
            _usuarioRepo = new UsuarioRepository();
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));

            Productos = new ObservableCollection<Producto>();
            Usuarios = new ObservableCollection<Usuario>();

            
            WindowLoadedCommand = new ViewModelCommand(OnWindowLoaded);
            ShowInventarioCommand = new ViewModelCommand(OnShowInventario);
            ShowUsuariosCommand = new ViewModelCommand(OnShowUsuarios);
            CerrarSesionCommand = new ViewModelCommand(OnCerrarSesion);

            AgregarProductoCommand = new ViewModelCommand(OnAgregarProducto);
            EliminarProductoCommand = new ViewModelCommand(OnEliminarProducto);
            EditarProductoCommand = new ViewModelCommand(OnEditarProducto);

            AgregarUsuarioCommand = new ViewModelCommand(OnAgregarUsuario);
            EliminarUsuarioCommand = new ViewModelCommand(OnEliminarUsuario);
            EditarUsuarioCommand = new ViewModelCommand(OnEditarUsuario);
        }

        // --- Métodos de Carga ---
        private void OnWindowLoaded(object obj)
        {
            CargarInventario();
        }

        private void CargarInventario()
        {
            try
            {
                var lista = _productoRepo.GetProductos();
                Productos.Clear();
                foreach (var p in lista) Productos.Add(p);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar el inventario: " + ex.Message,
                                "Error de Conexión", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CargarUsuarios()
        {
            try
            {
                var lista = _usuarioRepo.GetUsuarios();
                Usuarios.Clear();
                foreach (var u in lista) Usuarios.Add(u);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar los usuarios: " + ex.Message,
                                "Error de Conexión", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // --- Lógica de Navegación (RadioButtons) ---
        private void OnShowInventario(object obj)
        {
            InventarioVisibility = Visibility.Visible;
            PanelBotonesProductosVisibility = Visibility.Visible;

            UsuariosVisibility = Visibility.Collapsed;
            PanelBotonesUsuariosVisibility = Visibility.Collapsed;
        }

        private void OnShowUsuarios(object obj)
        {
            InventarioVisibility = Visibility.Collapsed;
            PanelBotonesProductosVisibility = Visibility.Collapsed;

            UsuariosVisibility = Visibility.Visible;
            PanelBotonesUsuariosVisibility = Visibility.Visible;

            CargarUsuarios(); 
        }

        // --- Lógica de Productos ---
        private void OnAgregarProducto(object obj)
        {
            bool? resultado = _dialogService.ShowAgregarProducto();
            if (resultado == true)
            {
                CargarInventario();
            }
        }

        private void OnEliminarProducto(object obj)
        {
            
            if (SelectedProducto == null)
            {
                MessageBox.Show("Por favor, selecciona un producto de la lista para eliminar.",
                                "Ningún producto seleccionado", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string nombreProducto = SelectedProducto.NOMBRE_PRODUCTO;
            MessageBoxResult confirmacion = MessageBox.Show(
                $"¿Estás seguro de que deseas eliminar el producto: '{nombreProducto}'?",
                "Confirmar Eliminación", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (confirmacion == MessageBoxResult.No) return;

            try
            {
                bool success = _productoRepo.DeleteProducto(SelectedProducto.ID_PRODUCTO);
                if (success)
                {
                    MessageBox.Show("Producto eliminado exitosamente.");
                    CargarInventario();
                }
                else
                {
                    MessageBox.Show("No se pudo eliminar el producto.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al eliminar el producto: " + ex.Message,
                                "Error de Base de Datos", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            
        }

        private void OnEditarProducto(object obj)
        {
            
            if (SelectedProducto == null)
            {
                MessageBox.Show("Por favor, selecciona un producto de la lista para editar.",
                                "Ningún producto seleccionado", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            bool? resultado = _dialogService.ShowEditarProducto(SelectedProducto);
            if (resultado == true)
            {
                CargarInventario();
            }
            
        }

        // --- Lógica de Usuarios ---
        private void OnAgregarUsuario(object obj)
        {
            bool? resultado = _dialogService.ShowAgregarUsuario();
            if (resultado == true)
            {
                CargarUsuarios();
            }
        }

        private void OnEliminarUsuario(object obj)
        {
            
            if (SelectedUsuario == null)
            {
                MessageBox.Show("Por favor, selecciona un usuario de la lista para eliminar.",
                                "Ningún usuario seleccionado", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string nombreUsuario = SelectedUsuario.NOMBRE_USUARIO;

            if (nombreUsuario.Equals("admin", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("No se puede eliminar al usuario administrador principal.",
                                "Operación no permitida", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            MessageBoxResult confirmacion = MessageBox.Show(
                $"¿Estás seguro de que deseas eliminar al usuario: '{nombreUsuario}'?",
                "Confirmar Eliminación", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (confirmacion == MessageBoxResult.No) return;

            try
            {
                bool success = _usuarioRepo.DeleteUsuario(SelectedUsuario.ID_USUARIO);
                if (success)
                {
                    MessageBox.Show("Usuario eliminado exitosamente.");
                    CargarUsuarios();
                }
                else
                {
                    MessageBox.Show("No se pudo eliminar el usuario.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al eliminar el usuario: " + ex.Message,
                                "Error de Base de Datos", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            
        }

        private void OnEditarUsuario(object obj)
        {
            
            if (SelectedUsuario == null)
            {
                MessageBox.Show("Por favor, selecciona un usuario de la lista para editar.",
                                "Ningún usuario seleccionado", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            bool? resultado = _dialogService.ShowEditarUsuario(SelectedUsuario);
            if (resultado == true)
            {
                CargarUsuarios();
            }
            
        }

        // --- Lógica de Sesión ---
        private void OnCerrarSesion(object obj)
        {
            
            MessageBoxResult result = MessageBox.Show("¿Estás seguro de que deseas cerrar sesión?",
                                                      "Cerrar Sesión",
                                                      MessageBoxButton.YesNo,
                                                      MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                RequestLogout?.Invoke();
            }
            
        }
    }
}