using SistemaPV.Model;
using SistemaPV.Repositories;
using SistemaPV.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SistemaPV.View_Model
{
    public class VentaViewModel : ViewModelBase
    {
        // --- Campos ---
        private readonly VentaRepository _repository;
        private readonly int _loggedInUserId;
        private List<Producto> _masterProductosLista;
        private List<Producto> _masterInventarioLista; 


        // --- Propiedades de Navegación ---
        private Visibility _panelVentaVisibility = Visibility.Visible;
        public Visibility PanelVentaVisibility
        {
            get => _panelVentaVisibility;
            set { _panelVentaVisibility = value; OnPropertyChanged(nameof(PanelVentaVisibility)); }
        }

        private Visibility _panelHistorialVisibility = Visibility.Collapsed;
        public Visibility PanelHistorialVisibility
        {
            get => _panelHistorialVisibility;
            set { _panelHistorialVisibility = value; OnPropertyChanged(nameof(PanelHistorialVisibility)); }
        }

        private Visibility _panelInventarioVisibility = Visibility.Collapsed;
        public Visibility PanelInventarioVisibility
        {
            get => _panelInventarioVisibility;
            set { _panelInventarioVisibility = value; OnPropertyChanged(nameof(PanelInventarioVisibility)); }
        }

        private Visibility _panelDetallesVentaVisibility = Visibility.Collapsed;
        public Visibility PanelDetallesVentaVisibility
        {
            get => _panelDetallesVentaVisibility;
            set { _panelDetallesVentaVisibility = value; OnPropertyChanged(nameof(PanelDetallesVentaVisibility)); }
        }


        // --- Propiedades Panel Venta ---
        private ObservableCollection<Producto> _productosFiltradosCombo;
        public ObservableCollection<Producto> ProductosFiltradosCombo
        {
            get => _productosFiltradosCombo;
            set { _productosFiltradosCombo = value; OnPropertyChanged(nameof(ProductosFiltradosCombo)); }
        }

        private Producto _selectedProductoCombo;
        public Producto SelectedProductoCombo
        {
            get => _selectedProductoCombo;
            set { _selectedProductoCombo = value; OnPropertyChanged(nameof(SelectedProductoCombo)); }
        }

        private string _textoBusquedaProducto;
        public string TextoBusquedaProducto
        {
            get => _textoBusquedaProducto;
            set { _textoBusquedaProducto = value; OnPropertyChanged(nameof(TextoBusquedaProducto)); FiltrarProductoComboBox(); }
        }

        private string _cantidadVenta = "1";
        public string CantidadVenta
        {
            get => _cantidadVenta;
            set { _cantidadVenta = value; OnPropertyChanged(nameof(CantidadVenta)); }
        }

        private ObservableCollection<VentaDetalleItem> _carrito;
        public ObservableCollection<VentaDetalleItem> Carrito
        {
            get => _carrito;
            set { _carrito = value; OnPropertyChanged(nameof(Carrito)); }
        }

        private VentaDetalleItem _selectedCarritoItem;
        public VentaDetalleItem SelectedCarritoItem
        {
            get => _selectedCarritoItem;
            set { _selectedCarritoItem = value; OnPropertyChanged(nameof(SelectedCarritoItem)); }
        }

        private string _totalVenta = "$ 0.00";
        public string TotalVenta
        {
            get => _totalVenta;
            set { _totalVenta = value; OnPropertyChanged(nameof(TotalVenta)); }
        }

        private DateTime? _fechaVenta = DateTime.Now;
        public DateTime? FechaVenta
        {
            get => _fechaVenta;
            set { _fechaVenta = value; OnPropertyChanged(nameof(FechaVenta)); }
        }


        // --- Propiedades Panel Historial ---
        private ObservableCollection<VentaHistorial> _historialVentas;
        public ObservableCollection<VentaHistorial> HistorialVentas
        {
            get => _historialVentas;
            set { _historialVentas = value; OnPropertyChanged(nameof(HistorialVentas)); }
        }

        private VentaHistorial _selectedHistorialItem;
        public VentaHistorial SelectedHistorialItem
        {
            get => _selectedHistorialItem;
            set { _selectedHistorialItem = value; OnPropertyChanged(nameof(SelectedHistorialItem)); PanelDetallesVentaVisibility = Visibility.Collapsed; }
        }

        private ObservableCollection<VentaHistorialDetalle> _detallesVenta;
        public ObservableCollection<VentaHistorialDetalle> DetallesVenta
        {
            get => _detallesVenta;
            set { _detallesVenta = value; OnPropertyChanged(nameof(DetallesVenta)); }
        }


        // --- Propiedades Panel Inventario ---
        private ObservableCollection<Producto> _inventarioFiltrado;
        public ObservableCollection<Producto> InventarioFiltrado
        {
            get => _inventarioFiltrado;
            set { _inventarioFiltrado = value; OnPropertyChanged(nameof(InventarioFiltrado)); }
        }

        private string _textoBusquedaInventario;
        public string TextoBusquedaInventario
        {
            get => _textoBusquedaInventario;
            set { _textoBusquedaInventario = value; OnPropertyChanged(nameof(TextoBusquedaInventario)); FiltrarInventario(); }
        }


        public event Action RequestLogout;

        // --- Comandos ---
        public ViewModelCommand WindowLoadedCommand { get; private set; }
        public ViewModelCommand CerrarSesionCommand { get; private set; }


        // Navegación
        public ViewModelCommand ShowVentaViewCommand { get; private set; }
        public ViewModelCommand ShowHistorialViewCommand { get; private set; }
        public ViewModelCommand ShowInventarioViewCommand { get; private set; }


        // Panel Venta
        public ViewModelCommand IncrementarCantidadCommand { get; private set; }
        public ViewModelCommand DecrementarCantidadCommand { get; private set; }
        public ViewModelCommand AgregarAlCarritoCommand { get; private set; }
        public ViewModelCommand QuitarDelCarritoCommand { get; private set; }
        public ViewModelCommand CancelarVentaCommand { get; private set; }
        public ViewModelCommand ProcederVentaCommand { get; private set; }


        // Panel Historial
        public ViewModelCommand VerDetallesVentaCommand { get; private set; }
        public ViewModelCommand CancelarVentaHistorialCommand { get; private set; }


        // Panel Inventario
        public ViewModelCommand LimpiarBusquedaInventarioCommand { get; private set; }


        // --- Constructor ---
        public VentaViewModel(int idUsuario)
        {
            _repository = new VentaRepository();
            _loggedInUserId = idUsuario;
            _masterProductosLista = new List<Producto>();
            _masterInventarioLista = new List<Producto>();

            // Inicializar colecciones
            ProductosFiltradosCombo = new ObservableCollection<Producto>();
            Carrito = new ObservableCollection<VentaDetalleItem>();
            HistorialVentas = new ObservableCollection<VentaHistorial>();
            DetallesVenta = new ObservableCollection<VentaHistorialDetalle>();
            InventarioFiltrado = new ObservableCollection<Producto>();

            // Registrar Comandos
            WindowLoadedCommand = new ViewModelCommand(OnWindowLoaded);
            CerrarSesionCommand = new ViewModelCommand(OnCerrarSesion);

            ShowVentaViewCommand = new ViewModelCommand(OnShowVentaView);
            ShowHistorialViewCommand = new ViewModelCommand(OnShowHistorialView);
            ShowInventarioViewCommand = new ViewModelCommand(OnShowInventarioView);

            IncrementarCantidadCommand = new ViewModelCommand(OnIncrementarCantidad);
            DecrementarCantidadCommand = new ViewModelCommand(OnDecrementarCantidad);
            AgregarAlCarritoCommand = new ViewModelCommand(OnAgregarAlCarrito);
            QuitarDelCarritoCommand = new ViewModelCommand(OnQuitarDelCarrito);
            CancelarVentaCommand = new ViewModelCommand(OnCancelarVenta);
            ProcederVentaCommand = new ViewModelCommand(OnProcederVenta);

            VerDetallesVentaCommand = new ViewModelCommand(OnVerDetallesVenta);
            CancelarVentaHistorialCommand = new ViewModelCommand(OnCancelarVentaHistorial);

            LimpiarBusquedaInventarioCommand = new ViewModelCommand(OnLimpiarBusquedaInventario);
        }

        // --- Métodos de Carga ---
        private void OnWindowLoaded(object obj)
        {
            try
            {
                _masterProductosLista = _repository.GetTodosProductos();
                ProductosFiltradosCombo = new ObservableCollection<Producto>(_masterProductosLista);

                _masterInventarioLista = new List<Producto>(_masterProductosLista); // Reutilizamos la lista
                InventarioFiltrado = new ObservableCollection<Producto>(_masterInventarioLista);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error fatal al cargar productos: " + ex.Message, "Error de Conexión", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        // --- Métodos de Navegación ---
        private void OnShowVentaView(object obj)
        {
            PanelVentaVisibility = Visibility.Visible;
            PanelHistorialVisibility = Visibility.Collapsed;
            PanelInventarioVisibility = Visibility.Collapsed;
            PanelDetallesVentaVisibility = Visibility.Collapsed;
        }

        private void OnShowHistorialView(object obj)
        {
            PanelVentaVisibility = Visibility.Collapsed;
            PanelHistorialVisibility = Visibility.Visible;
            PanelInventarioVisibility = Visibility.Collapsed;
            PanelDetallesVentaVisibility = Visibility.Collapsed;
            CargarHistorialVentas(); // Cargar al ver
        }

        private void OnShowInventarioView(object obj)
        {
            PanelVentaVisibility = Visibility.Collapsed;
            PanelHistorialVisibility = Visibility.Collapsed;
            PanelInventarioVisibility = Visibility.Visible;
            PanelDetallesVentaVisibility = Visibility.Collapsed;
            // Los datos ya están cargados desde OnWindowLoaded
        }


        // --- Métodos Panel Venta ---

        private void FiltrarProductoComboBox()
        {
            if (string.IsNullOrEmpty(TextoBusquedaProducto))
            {
                ProductosFiltradosCombo = new ObservableCollection<Producto>(_masterProductosLista);
            }
            else
            {
                // Copiamos la lógica exacta de FiltrarInventario()
                string filtro = TextoBusquedaProducto.Trim();
                var filtrados = _masterProductosLista
                    .Where(p => (p.NOMBRE_PRODUCTO != null && p.NOMBRE_PRODUCTO.IndexOf(filtro, StringComparison.OrdinalIgnoreCase) >= 0) ||
                                (p.DESCRIPCION != null && p.DESCRIPCION.IndexOf(filtro, StringComparison.OrdinalIgnoreCase) >= 0)) // <--- ¡AQUÍ ESTÁ LA MAGIA!
                    .ToList();
                ProductosFiltradosCombo = new ObservableCollection<Producto>(filtrados);
            }
        }

        private void OnIncrementarCantidad(object obj)
        {
            if (int.TryParse(CantidadVenta, out int cantidad))
            {
                cantidad++;
                CantidadVenta = cantidad.ToString();
            }
            else
            {
                CantidadVenta = "1";
            }
        }

        private void OnDecrementarCantidad(object obj)
        {
            if (int.TryParse(CantidadVenta, out int cantidad) && cantidad > 1)
            {
                cantidad--;
                CantidadVenta = cantidad.ToString();
            }
        }

        private void OnAgregarAlCarrito(object obj)
        {
            if (SelectedProductoCombo == null)
            {
                MessageBox.Show("Seleccione un producto.");
                return;
            }

            if (!int.TryParse(CantidadVenta, out int cantidad) || cantidad <= 0)
            {
                MessageBox.Show("Ingrese una cantidad válida.");
                return;
            }

            var productoEnStock = _masterProductosLista.FirstOrDefault(p => p.ID_PRODUCTO == SelectedProductoCombo.ID_PRODUCTO);
            if (productoEnStock == null)
            {
                MessageBox.Show("Error: Producto no encontrado en la lista maestra.");
                return;
            }


            // Validar stock (contra la lista maestra)
            if (cantidad > productoEnStock.CANTIDAD_STOCK)
            {
                MessageBox.Show("No hay suficiente stock disponible.");
                return;
            }


            // Agregar al carrito
            var itemCarrito = new VentaDetalleItem
            {
                Numero = Carrito.Count + 1,
                ProductoId = productoEnStock.ID_PRODUCTO,
                Nombre = productoEnStock.NOMBRE_PRODUCTO,
                Cantidad = cantidad,
                Precio = productoEnStock.PRECIO
            };
            Carrito.Add(itemCarrito);


            // Actualizar stock localmente (para que no se pueda volver a agregar)
            productoEnStock.CANTIDAD_STOCK -= cantidad;

            CalcularTotal();
            CantidadVenta = "1";
            TextoBusquedaProducto = string.Empty; // Limpiar combobox
        }

        private void OnQuitarDelCarrito(object obj)
        {
            if (SelectedCarritoItem == null)
            {
                MessageBox.Show("Por favor, selecciona un producto de la lista para quitar.",
                                "Ningún producto seleccionado", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MessageBoxResult confirmacion = MessageBox.Show(
                $"¿Estás seguro de que deseas quitar el producto '{SelectedCarritoItem.Nombre}' de la venta?",
                "Confirmar Eliminación", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (confirmacion != MessageBoxResult.Yes) return;


            // Devolver stock a la lista maestra local
            var productoEnStock = _masterProductosLista.FirstOrDefault(p => p.ID_PRODUCTO == SelectedCarritoItem.ProductoId);
            if (productoEnStock != null)
            {
                productoEnStock.CANTIDAD_STOCK += SelectedCarritoItem.Cantidad;
            }

            Carrito.Remove(SelectedCarritoItem);


            // Renumerar
            int i = 1;
            foreach (var item in Carrito)
            {
                item.Numero = i++;
            }

            CalcularTotal();
        }

        private void CalcularTotal()
        {
            decimal total = Carrito.Sum(i => i.Subtotal);
            TotalVenta = $"$ {total:F2}";
        }

        private void OnCancelarVenta(object obj)
        {
            if (MessageBox.Show("¿Deseas cancelar la venta actual?", "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;


            // Devolver stock de todo el carrito
            foreach (var item in Carrito)
            {
                var productoEnStock = _masterProductosLista.FirstOrDefault(p => p.ID_PRODUCTO == item.ProductoId);
                if (productoEnStock != null)
                {
                    productoEnStock.CANTIDAD_STOCK += item.Cantidad;
                }
            }

            Carrito.Clear();
            CalcularTotal();
            CantidadVenta = "1";
        }

        private void OnProcederVenta(object obj)
        {
            if (!Carrito.Any())
            {
                MessageBox.Show("No hay productos en la venta.");
                return;
            }
            if (!FechaVenta.HasValue)
            {
                MessageBox.Show("Seleccione una fecha válida.");
                return;
            }

            // Calculamos el total para pasarlo a la ventana de pago
            decimal totalActualVenta = Carrito.Sum(i => i.Subtotal);

            var pagarVentaViewModel = new PagarVentaViewModel(totalActualVenta);
            var pagarVentaWindow = new PagarVentaView(pagarVentaViewModel); 
            pagarVentaWindow.Owner = Application.Current.MainWindow; 

            bool? dialogResult = pagarVentaWindow.ShowDialog();

            if (dialogResult != true)
            {
                return; 
            }

            int metodoPagoId = pagarVentaViewModel.SelectedMetodoPagoId;

            try
            {
                // FECHA del calendario
                DateTime fechaDeCalendario = FechaVenta.Value.Date;
                // HORA actual
                TimeSpan horaActual = DateTime.Now.TimeOfDay;

                DateTime fechaYHoraCorrecta = fechaDeCalendario + horaActual;

                bool success = _repository.RegistrarVenta(Carrito.ToList(), fechaYHoraCorrecta, _loggedInUserId, metodoPagoId);

                if (success)
                {
                    MessageBox.Show("Venta registrada exitosamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                    Carrito.Clear();
                    CalcularTotal();
                    CantidadVenta = "1";

                    // Recargar los productos para actualizar stock y el historial de ventas
                    OnWindowLoaded(null); // Recarga los productos (stock)
                    if (PanelHistorialVisibility == Visibility.Visible)
                    {
                        CargarHistorialVentas();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al registrar la venta: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                // Si la venta falla, debemos devolver el stock a la lista local
                OnCancelarVenta(null);
            }
        }


        // --- Métodos Panel Historial ---
        private void CargarHistorialVentas()
        {
            try
            {
                var historial = _repository.GetHistorialVentas();
                HistorialVentas = new ObservableCollection<VentaHistorial>(historial);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar el historial de ventas: " + ex.Message);
            }
        }

        private void OnVerDetallesVenta(object obj)
        {
            if (SelectedHistorialItem == null)
            {
                MessageBox.Show("Por favor, selecciona una venta del historial para ver sus detalles.",
                                "Ninguna venta seleccionada", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var detalles = _repository.GetDetallesDeVenta(SelectedHistorialItem.ID_VENTA);
                DetallesVenta = new ObservableCollection<VentaHistorialDetalle>(detalles);
                PanelDetallesVentaVisibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar los detalles de la venta: " + ex.Message);
            }
        }

        private void OnCancelarVentaHistorial(object obj)
        {
            if (SelectedHistorialItem == null)
            {
                MessageBox.Show("Por favor, selecciona una venta del historial para cancelar.",
                                "Ninguna venta seleccionada", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (SelectedHistorialItem.ESTADO_VENTA == "Cancelada")
            {
                MessageBox.Show("Esta venta ya está cancelada.",
                                "Venta ya cancelada", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            MessageBoxResult confirmacion = MessageBox.Show(
                $"¿Estás seguro de que deseas cancelar la venta #{SelectedHistorialItem.ID_VENTA}?\n\nEsta acción devolverá todos los productos al inventario.",
                "Confirmar Cancelación", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (confirmacion != MessageBoxResult.Yes) return;

            try
            {
                bool success = _repository.CancelarVenta(SelectedHistorialItem.ID_VENTA);
                if (success)
                {
                    MessageBox.Show("Venta cancelada exitosamente. Los productos han sido devueltos al inventario.",
                                    "Cancelación Exitosa", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Recargar todo
                    CargarHistorialVentas();
                    OnWindowLoaded(null); // Recarga productos y stock
                    PanelDetallesVentaVisibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cancelar la venta: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        // --- Métodos Panel Inventario ---
        private void OnLimpiarBusquedaInventario(object obj)
        {
            TextoBusquedaInventario = string.Empty;
        }

        private void FiltrarInventario()
        {
            if (string.IsNullOrEmpty(TextoBusquedaInventario))
            {
                InventarioFiltrado = new ObservableCollection<Producto>(_masterInventarioLista);
            }
            else
            {
                string filtro = TextoBusquedaInventario.Trim();
                var filtrados = _masterInventarioLista
                    .Where(p => (p.NOMBRE_PRODUCTO != null && p.NOMBRE_PRODUCTO.IndexOf(filtro, StringComparison.OrdinalIgnoreCase) >= 0) ||
                                (p.DESCRIPCION != null && p.DESCRIPCION.IndexOf(filtro, StringComparison.OrdinalIgnoreCase) >= 0))
                    .ToList();
                InventarioFiltrado = new ObservableCollection<Producto>(filtrados);
            }
        }


        // --- Método Cerrar Sesión ---
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
