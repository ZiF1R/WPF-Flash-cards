﻿using course_project1.controls;
using course_project1.controls.ModalWindows;
using course_project1.storage;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace course_project1.view
{
    /// <summary>
    /// Логика взаимодействия для RegistrationPage.xaml
    /// </summary>
    public partial class RegistrationPage : Page
    {
        string ConnectionString;
        DataStorage Storage;
        Grid MainWindowGrid;

        public RegistrationPage(Grid mainWindowGrid, string connectionString, DataStorage storage)
        {
            MainWindowGrid = mainWindowGrid;
            this.ConnectionString = connectionString;
            this.Storage = storage;
            InitializeComponent();
        }

        private void Registrate_Click(object sender, RoutedEventArgs e)
        {
            bool isValid = ValidateForm();
            if (!isValid) return;

            try
            {
                Storage.user = new User(
                    NicknameInput.Value,
                    SurnameInput.Value,
                    NameInput.Value,
                    EmailInput.Value,
                    PasswordInput.Value,
                    ConnectionString
                );
                Storage.settings.CreateUserSettings(ConnectionString, Storage.user.Uid);
                NavigationService.Navigate(new LoginPage(MainWindowGrid, ConnectionString, Storage));
            }
            catch (Exception ex)
            {
                CustomMessage.Show(ex.Message);
            }
        }

        private bool ValidateForm()
        {
            try
            {
                NicknameInput.Value = NicknameInput.Value.Trim();
                SurnameInput.Value = SurnameInput.Value.Trim();
                NameInput.Value = NameInput.Value.Trim();

                Validator.ValidateInput(NicknameInput.Value, "NicknameFormatError", true);
                Validator.ValidateInput(SurnameInput.Value, "SurnameFormatError");
                Validator.ValidateInput(NameInput.Value, "NameFormatError");
                Validator.ValidateEmail(EmailInput.Value);
                Validator.ValidatePassword(PasswordInput.Value);

                if (PasswordInput.Value != ConfirmPasswordInput.Value)
                {
                    CustomMessage.Show((string)Application.Current.FindResource("ConfirmPasswordError"));
                    return false;
                }

                return true;
            }
            catch (FormatException ex)
            {
                CustomMessage.Show(ex.Message);
                return false;
            }
            catch (Exception ex)
            {
                CustomMessage.Show(ex.Message);
                return false;
            }
        }

        private void GoToLogin_MouseUp(object sender, MouseButtonEventArgs e)
        {
            NavigationService.Navigate(new LoginPage(MainWindowGrid, ConnectionString, Storage));
        }
    }
}
