using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using SistemaPV.Model;
using SistemaPV.Repositories;

namespace SistemaPV.View_Model
{
    public class PagarVentaViewModel : ViewModelBase
    {
        // --- Eventos para cerrar la ventana ---
        public event EventHandler RequestClose;
        public event EventHandler<bool?> RequestDialogResult;

        // --- Campos ---
        private readonly VentaRepository _repository; 
        private readonly decimal _totalVenta;

        // --- Propiedades ---
        private string _totalVentaDisplay;
        public string TotalVentaDisplay
        {
            get => _totalVentaDisplay;
            set { _totalVentaDisplay = value; OnPropertyChanged(nameof(TotalVentaDisplay)); }
        }

        private bool _isEfectivoSelected = true;
        public bool IsEfectivoSelected
        {
            get => _isEfectivoSelected;
            set { _isEfectivoSelected = value; OnPropertyChanged(nameof(IsEfectivoSelected)); OnPropertyChanged(nameof(EfectivoVisibility)); UpdateCambio(); }
        }

        public bool IsTarjetaSelected
        {
            get => !_isEfectivoSelected;
            set { _isEfectivoSelected = !value; OnPropertyChanged(nameof(IsTarjetaSelected)); OnPropertyChanged(nameof(EfectivoVisibility)); UpdateCambio(); }
        }

        public Visibility EfectivoVisibility => IsEfectivoSelected ? Visibility.Visible : Visibility.Collapsed;

        private string _montoPagado = string.Empty;

        public string MontoPagado
        {
        
            get => _montoPagado;

            set
            {
                if (_montoPagado == value) return; 
                _montoPagado = value;
                OnPropertyChanged(nameof(MontoPagado));
                UpdateCambio(); 
            }
        }

        private string _cambio = "0.00";
        public string Cambio
        {
            get => _cambio;
            set { _cambio = value; OnPropertyChanged(nameof(Cambio)); }
        }

        public int SelectedMetodoPagoId { get; private set; }

        // --- Comandos ---
        public ViewModelCommand CancelarCommand { get; private set; }
        public ViewModelCommand FinalizarCommand { get; private set; }

        // --- Constructor ---
        public PagarVentaViewModel(decimal totalVenta)
        {
            _repository = new VentaRepository();
            _totalVenta = totalVenta;
            TotalVentaDisplay = totalVenta.ToString("F2");

            CancelarCommand = new ViewModelCommand(OnCancelar);
            FinalizarCommand = new ViewModelCommand(OnFinalizar);

            UpdateCambio();
        }

        // --- Métodos de Lógica ---
        private void OnCancelar(object obj)
        {
            RequestDialogResult?.Invoke(this, false); // Indica que se canceló
            RequestClose?.Invoke(this, EventArgs.Empty);
        }

        private void OnFinalizar(object obj)
        {
            if (IsEfectivoSelected)
            {
                if (!decimal.TryParse(MontoPagado, out decimal pagado) || pagado < _totalVenta)
                {
                    MessageBox.Show("El monto pagado es insuficiente.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                SelectedMetodoPagoId = 1;
            }
            else
            {
                SelectedMetodoPagoId = 2;
            }

            RequestDialogResult?.Invoke(this, true);
            RequestClose?.Invoke(this, EventArgs.Empty);
        }

        private void UpdateCambio()
        {
            if (IsEfectivoSelected)
            {
                if (decimal.TryParse(MontoPagado, out decimal pagado))
                {
                    decimal cambioValue = pagado - _totalVenta;
 
                    Cambio = (cambioValue > 0) ? cambioValue.ToString("F2") : "0.00";
                }
                else
                {
                    Cambio = "0.00";
                }
            }
            else // Si es Tarjeta
            {
                Cambio = "0.00"; 

                if (_montoPagado != "0")
                {
                    _montoPagado = "0";
                    OnPropertyChanged(nameof(MontoPagado)); 
                }
            }
        }
    }
}