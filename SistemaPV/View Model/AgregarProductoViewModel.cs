using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SistemaPV.Model;
using SistemaPV.Repositories;
using System.Windows;

namespace SistemaPV.View_Model 
{
    public class AgregarProductoViewModel : ViewModelBase 
    {
        private readonly ProductoRepository _repository;

        // Propiedades para la Vista
        private string _nombre;
        public string Nombre
        {
            get => _nombre;
            set { _nombre = value; OnPropertyChanged(nameof(Nombre)); } 
        }

        private string _precio;
        public string Precio
        {
            get => _precio;
            set { _precio = value; OnPropertyChanged(nameof(Precio)); }
        }

        private string _stock;
        public string Stock
        {
            get => _stock;
            set { _stock = value; OnPropertyChanged(nameof(Stock)); }
        }

        private string _descripcion;
        public string Descripcion
        {
            get => _descripcion;
            set { _descripcion = value; OnPropertyChanged(nameof(Descripcion)); }
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

        // Estado
        private bool _isEditMode;
        private int? _productoId;

        // Evento para cerrar la ventana
        public event Action<bool?> RequestClose;

        // Comandos
        public ViewModelCommand GuardarCommand { get; private set; } 
        public ViewModelCommand CancelarCommand { get; private set; }

        // Constructor para "Agregar"
        public AgregarProductoViewModel()
        {
            _repository = new ProductoRepository();
            Titulo = "Agregar Nuevo Producto";
            BotonGuardarTexto = "Guardar";
            _isEditMode = false;

            GuardarCommand = new ViewModelCommand(OnGuardar); 
            CancelarCommand = new ViewModelCommand(OnCancelar);
        }

        // Constructor para "Editar"
        public AgregarProductoViewModel(Producto productoAEditar) : this()
        {
            _isEditMode = true;
            _productoId = productoAEditar.ID_PRODUCTO;

            Nombre = productoAEditar.NOMBRE_PRODUCTO;
            Precio = productoAEditar.PRECIO.ToString();
            Stock = productoAEditar.CANTIDAD_STOCK.ToString();
            Descripcion = productoAEditar.DESCRIPCION;

            Titulo = "Editar Producto";
            BotonGuardarTexto = "Actualizar";
        }

        private void OnGuardar(object obj)
        {
            
            if (string.IsNullOrEmpty(Nombre) || string.IsNullOrEmpty(Precio) || string.IsNullOrEmpty(Stock))
            {
                MessageBox.Show("Nombre, Precio y Stock son obligatorios.");
                return;
            }
            if (Nombre.Length > 150)
            {
                MessageBox.Show("El nombre no puede tener más de 150 caracteres.");
                return;
            }
            if (!string.IsNullOrEmpty(Descripcion) && Descripcion.Length > 500)
            {
                MessageBox.Show("La descripción no puede tener más de 500 caracteres.");
                return;
            }
            if (!decimal.TryParse(Precio, out decimal precioParsed) || precioParsed < 0)
            {
                MessageBox.Show("El precio debe ser un número decimal válido y mayor o igual a 0.");
                return;
            }
            if (!int.TryParse(Stock, out int stockParsed) || stockParsed < 0)
            {
                MessageBox.Show("El stock debe ser un número entero válido y mayor o igual a 0.");
                return;
            }
            

            var producto = new Producto
            {
                NOMBRE_PRODUCTO = this.Nombre,
                PRECIO = precioParsed,
                CANTIDAD_STOCK = stockParsed,
                DESCRIPCION = this.Descripcion ?? "" // Asegura no nulos
            };

            try
            {
                bool success;
                string successMessage;

                if (_isEditMode)
                {
                    producto.ID_PRODUCTO = _productoId.Value;
                    success = _repository.UpdateProducto(producto);
                    successMessage = "¡Producto actualizado exitosamente!";
                }
                else
                {
                    success = _repository.AddProducto(producto);
                    successMessage = "¡Producto guardado exitosamente!";
                }

                if (success)
                {
                    MessageBox.Show(successMessage);
                    RequestClose?.Invoke(true); // Cierra la ventana con DialogResult = true
                }
                else
                {
                    MessageBox.Show("No se pudo guardar el producto.");
                }
            }
            catch (FormatException) 
            {
                MessageBox.Show("Error: El precio y el stock deben ser números válidos.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar el producto: " + ex.Message);
            }
        }

        private void OnCancelar(object obj)
        {
            RequestClose?.Invoke(false); // Cierra la ventana con DialogResult = false
        }
    }
}
