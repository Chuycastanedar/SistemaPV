using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using SistemaPV.View;

namespace SistemaPV
{
    /// <summary>
    /// Lógica de interacción para App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static string RoleDelUsuarioLogueado { get; set; }
        public static int IdDelUsuarioLogueado { get; set; }


        protected void ApplicationStart(object sender, StartupEventArgs e)
        {
            var loginView = new LoginView();
            loginView.Show();

            loginView.IsVisibleChanged += (s, ev) =>
            {

                if (loginView.IsVisible == false && loginView.IsLoaded)
                {
                    string role = App.RoleDelUsuarioLogueado;

                    if (role == "Administrador")
                    {
                        Inventario inventario = new Inventario();
                        inventario.Show();
                    }
                    else if (role == "Cajero")
                    {
                        VentaWindow venta = new VentaWindow(App.IdDelUsuarioLogueado);
                        venta.Show();
                    }
                    else
                    {
                        MessageBox.Show("Rol de usuario no reconocido.");
                    }

                    loginView.Close();
                }
            };
        }


        public void NavigateToLogin()
        {
            var oldWindow = MainWindow;

            RoleDelUsuarioLogueado = null;
            IdDelUsuarioLogueado = 0;

            var loginView = new LoginView();
            MainWindow = loginView;
            loginView.Show();

            loginView.IsVisibleChanged += (s, ev) =>
            {
                if (loginView.IsVisible == false && loginView.IsLoaded)
                {
                    string role = App.RoleDelUsuarioLogueado;

                    if (role == "Administrador")
                    {
                        Inventario inventario = new Inventario();
                        MainWindow = inventario;
                        inventario.Show();
                    }
                    else if (role == "Cajero")
                    {
                        VentaWindow venta = new VentaWindow(App.IdDelUsuarioLogueado);
                        MainWindow = venta;
                        venta.Show();
                    }
                    else
                    {
                        // No hacer nada si el rol está vacío (solo se cerró el login)
                    }

                    loginView.Close();
                }
            };

            if (oldWindow != null)
            {
                oldWindow.Close(); 
            }
        }
    }
}

