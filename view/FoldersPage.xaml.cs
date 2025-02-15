﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
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
using course_project1.controls;
using course_project1.controls.ModalWindows;
using course_project1.storage;

namespace course_project1.view
{
    /// <summary>
    /// Логика взаимодействия для FoldersPage.xaml
    /// </summary>
    public partial class FoldersPage : Page
    {
        DataStorage Storage;
        string ConnectionString;
        Frame SecondFrame;
        Grid MainPageGrid;
        Folder[] filteredFolders;
        bool isModalOpened = false;

        FolderControl.Action action;

        public FoldersPage(
            Grid mainPageGrid, Frame secondFrame,
            string connectionString, DataStorage storage,
            FolderControl.Action action = FolderControl.Action.None)
        {
            this.action = action;
            ConnectionString = connectionString;
            Storage = storage;
            MainPageGrid = mainPageGrid;
            SecondFrame = secondFrame;
            filteredFolders = Storage.folders;
            InitializeComponent();

            foreach (Folder folder in Storage.folders)
                FoldersWrap.Children.Insert(1, this.CreateFolderElement(folder));

            foreach (Category category in Storage.categories)
                CategorySearch.Items.Add(category.Name);

            if (action != FolderControl.Action.None)
            {
                AddCard.Visibility = Visibility.Collapsed;
            }
        }

        private FolderControl CreateFolderElement(Folder folder)
        {
            FolderControl folderControl = null;
            try
            {
                folderControl = new FolderControl(MainPageGrid, folder, Storage, ConnectionString, action);
                folderControl.Margin = new Thickness(0, 0, 40, 40);
                folderControl.VerticalAlignment = VerticalAlignment.Top;

                folderControl.EditFolder += (object s, RoutedEventArgs ev) =>
                {
                    if (isModalOpened) return;

                    FolderModalWindow modal = new FolderModalWindow(
                        MainPageGrid, ConnectionString, Storage, folderControl.FolderNameField.Text, folderControl.FolderCategoryField.Text);
                    modal.SetValue(Grid.RowSpanProperty, 2);
                    modal.SetValue(Grid.ColumnSpanProperty, 3);
                    modal.SetResourceReference(FolderModalWindow.ModalHeaderProperty, "EditFolder");
                    modal.SetResourceReference(FolderModalWindow.ActionButtonContentProperty, "Edit");
                    MainPageGrid.Children.Add(modal);
                    isModalOpened = true;

                    modal.FolderAction += (object se, RoutedEventArgs evn) =>
                    {
                        folder.ChangeFolderData(ConnectionString, Storage.user.Uid, modal.FolderName, modal.FolderCategory);

                        folderControl.FolderNameField.Text = modal.FolderName;
                        folderControl.FolderCategoryField.Text = modal.FolderCategory;
                    };
                    modal.CloseFolderModal += (object se, RoutedEventArgs evn) =>
                    {
                        MainPageGrid.Children.Remove(modal);
                        isModalOpened = false;

                        if (Storage.categories.Length != CategorySearch.Items.Count - 1)
                        {
                            CategorySearch.Items.Clear();

                            ComboBoxItem item = new ComboBoxItem();
                            item.Content = (string)Application.Current.FindResource("ResetCategory");
                            item.IsSelected = true;
                            CategorySearch.Items.Add(item);

                            foreach (Category category in Storage.categories)
                                CategorySearch.Items.Add(category.Name);
                        }
                    };
                };
                folderControl.RemoveFolder += (object s, RoutedEventArgs ev) =>
                {
                    if (isModalOpened) return;

                    bool isSuccessfully = folder.RemoveFolder(ConnectionString, Storage.user.Uid);
                    if (!isSuccessfully) return;

                    Storage.folders = Storage.folders.Where(x => x.Name != folderControl.FolderName).ToArray();
                    FoldersWrap.Children.Remove(folderControl);
                };
                folderControl.GoToCards += (object s, RoutedEventArgs ev) =>
                    SecondFrame.Content = new CardsView(MainPageGrid, SecondFrame, folder, ConnectionString, Storage);
                folderControl.ReturnToSettings += (object s, RoutedEventArgs ev) =>
                    SecondFrame.Content = new SettingsPage(MainPageGrid, ConnectionString, Storage, SecondFrame);
            }
            catch
            {
                CustomMessage.Show((string)Application.Current.FindResource("FolderCreateError"));
            }
            return folderControl;
        }

        private void AddFolderButton_AddFolder(object sender, RoutedEventArgs e)
        {
            if (isModalOpened) return;

            FolderModalWindow modal = new FolderModalWindow(MainPageGrid, ConnectionString, Storage, "", "");
            modal.SetValue(Grid.RowSpanProperty, 2);
            modal.SetValue(Grid.ColumnSpanProperty, 3);
            modal.SetResourceReference(FolderModalWindow.ModalHeaderProperty, "CreateFolder");
            modal.SetResourceReference(FolderModalWindow.ActionButtonContentProperty, "Create");

            MainPageGrid.Children.Add(modal);
            isModalOpened = true;

            modal.FolderAction += (object s, RoutedEventArgs ev) =>
            {
                string folderName = modal.FolderName;
                string folderCategory = modal.FolderCategory;

                try
                {
                    Folder folder = new Folder(ConnectionString, Storage.user.Uid, folderName, folderCategory);
                    Storage.folders = Storage.folders.Append(folder).ToArray();

                    if (folder.Name.Contains(SearchInput.Value))
                    {
                        if (CategorySearch.SelectedIndex == 0 ||
                            folder.Category.ToString() ==
                            CategorySearch.Items.GetItemAt(CategorySearch.SelectedIndex).ToString())
                        {
                            FoldersWrap.Children.Insert(1, this.CreateFolderElement(folder));
                        }
                    }
                }
                catch { }
            };
            modal.CloseFolderModal += (object s, RoutedEventArgs ev) =>
            {
                MainPageGrid.Children.Remove(modal);
                isModalOpened = false;

                if (Storage.categories.Length != CategorySearch.Items.Count - 1)
                {
                    CategorySearch.Items.Clear();

                    ComboBoxItem item = new ComboBoxItem();
                    item.Content = (string)Application.Current.FindResource("ResetCategory");
                    item.IsSelected = true;
                    CategorySearch.Items.Add(item);

                    foreach (Category category in Storage.categories)
                        CategorySearch.Items.Add(category.Name);
                }
            };
        }

        private void SearchInput_Input(object sender, RoutedEventArgs e)
        {
            SearchInput.Value = SearchInput.Value.Trim();
            FoldersWrap.Children.RemoveRange(1, FoldersWrap.Children.Count - 1);
            filteredFolders = Storage.folders.Where(f => f.Name.Contains(SearchInput.Value)).ToArray();

            if (CategorySearch.SelectedIndex != 0)
                filteredFolders = filteredFolders.Where(f => f.Category == CategorySearch.Text).ToArray();

            foreach (Folder folder in filteredFolders)
                FoldersWrap.Children.Insert(1, this.CreateFolderElement(folder));
        }

        private void CategorySearch_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (FoldersWrap == null) return;

                FoldersWrap.Children.RemoveRange(1, FoldersWrap.Children.Count - 1);
                filteredFolders = Storage.folders.Where(f => f.Name.Contains(SearchInput.Value)).ToArray();

                if (CategorySearch.SelectedIndex != 0)
                    filteredFolders = filteredFolders.Where(f =>
                        f.Category.ToString() == CategorySearch.Items.GetItemAt(CategorySearch.SelectedIndex).ToString()).ToArray();

                foreach (Folder folder in filteredFolders)
                    FoldersWrap.Children.Insert(1, this.CreateFolderElement(folder));
            } catch { }
        }
    }
}
