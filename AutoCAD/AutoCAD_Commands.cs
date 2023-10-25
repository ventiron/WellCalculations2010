﻿using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Ribbon;
using Autodesk.AutoCAD.Runtime;
using Autodesk.Windows;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.OleDb;
using System.Linq;
using System.Runtime.Remoting;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using WellCalculations2010.Model;
using WellCalculations2010.View;

using Application = Autodesk.AutoCAD.ApplicationServices.Application;


namespace WellCalculations2010.AutoCAD
{
    public class AutoCAD_Commands : Autodesk.AutoCAD.Runtime.IExtensionApplication
    {
        private static SectionDrawer_Window _SectionDrawer_Window;
        private static WellPlanarImport_Window _WellPlanarImport_Window;



        [CommandMethod("РРР")]
        public void DrawSection_Command()
        {
            if(_SectionDrawer_Window == null || !_SectionDrawer_Window.IsLoaded)
                _SectionDrawer_Window = new SectionDrawer_Window();
            if (_SectionDrawer_Window.IsLoaded && _SectionDrawer_Window.WindowState == System.Windows.WindowState.Minimized)
                _SectionDrawer_Window.WindowState = System.Windows.WindowState.Normal;
            else
                _SectionDrawer_Window.Show();
        }

        [CommandMethod("ИмпортСкважин")]
        public void ImportWells()
        {
            if (_WellPlanarImport_Window == null || !_WellPlanarImport_Window.IsLoaded)
                _WellPlanarImport_Window = new WellPlanarImport_Window();
            if (_WellPlanarImport_Window.IsLoaded && _WellPlanarImport_Window.WindowState == System.Windows.WindowState.Minimized)
                _WellPlanarImport_Window.WindowState = System.Windows.WindowState.Normal;
            else
               _WellPlanarImport_Window.Show();
        }


        private static readonly string TabName = "InenTironAddin";
        private static readonly string TabId = "InenTironAddin_RibbonId";

        private static readonly string PanelName = "Работа со скважинами";
        private static readonly string PanelId = "WellCalculation_RibbonPanelId";

        public void Initialize()
        {
            Autodesk.Windows.ComponentManager.ItemInitialized += new EventHandler <RibbonItemEventArgs> (ComponentManager_ItemInitialized);
        }

        public void Terminate()
        {
            
        }

        //Событие для добавления вкладки в ленту
        void ComponentManager_ItemInitialized(object sender, Autodesk.Windows.RibbonItemEventArgs e)
        {
            // Проверяем, что лента загружена
            if (Autodesk.Windows.ComponentManager.Ribbon != null)
            {
                BuildRibbonTab();
                Autodesk.Windows.ComponentManager.ItemInitialized -= new EventHandler<RibbonItemEventArgs> (ComponentManager_ItemInitialized);
            }
        }

        void BuildRibbonTab()
        {
            // Если лента еще не загружена
            if (getAddinRibbonTab() == null)
            {
                // Строим вкладку
                CreateRibbonTab();
                RibbonTab tab = getAddinRibbonTab();
                AddContent(tab);
                ComponentManager.Ribbon.UpdateLayout();
                Application.SystemVariableChanged -= new Autodesk.AutoCAD.ApplicationServices.SystemVariableChangedEventHandler(acadApp_SystemVariableChanged);
                Application.SystemVariableChanged += new Autodesk.AutoCAD.ApplicationServices.SystemVariableChangedEventHandler(acadApp_SystemVariableChanged);
            }
            else
            {
                RibbonTab tab = getAddinRibbonTab();
                bool isDataCreated = false;
                foreach (RibbonPanel panel in tab.Panels)
                {
                    if (panel.Source.Id.Equals(PanelId) && panel.Source.Title.Equals(PanelName))
                    {
                        MessageBox.Show(panel.Source.Id);
                        isDataCreated = true;
                        break;
                    }
                }
                if (!isDataCreated)
                {
                    AddContent(tab);
                    ComponentManager.Ribbon.UpdateLayout();
                    Application.SystemVariableChanged -= new Autodesk.AutoCAD.ApplicationServices.SystemVariableChangedEventHandler(acadApp_SystemVariableChanged);
                    Application.SystemVariableChanged += new Autodesk.AutoCAD.ApplicationServices.SystemVariableChangedEventHandler(acadApp_SystemVariableChanged);
                }
            }
            // Подключаем обработчик событий изменения системных переменных

        }

        void acadApp_SystemVariableChanged(object sender, Autodesk.AutoCAD.ApplicationServices.SystemVariableChangedEventArgs e)
        {
            if (e.Name.Equals("WSCURRENT")) BuildRibbonTab();
        }

        RibbonTab getAddinRibbonTab()
        {
            RibbonControl ribCntrl = Autodesk.Windows.ComponentManager.Ribbon;
            foreach (RibbonTab tab in ribCntrl.Tabs)
            {
                // И если у вкладки совпадает идентификатор и заголовок, то значит вкладка загружена
                if (tab.Id.Equals(TabId) & tab.Title.Equals(TabName))
                    return tab; 
            }
            return null;
        }

        // Создание нашей вкладки
        void CreateRibbonTab()
        {
            try
            {
                // Получаем доступ к ленте
                RibbonControl ribCntrl = Autodesk.Windows.ComponentManager.Ribbon;
                // добавляем свою вкладку
                RibbonTab ribTab = new RibbonTab();
                ribTab.Title = TabName; // Заголовок вкладки
                ribTab.Id = TabId; // Идентификатор вкладки
                ribCntrl.Tabs.Add(ribTab);
            }
            catch (System.Exception ex)
            {
                Autodesk.AutoCAD.ApplicationServices.Application.
                  DocumentManager.MdiActiveDocument.Editor.WriteMessage(ex.Message);
            }
        }


        void AddContent(RibbonTab ribTab)
        {
            try
            {
                // создаем panel source
                RibbonPanelSource ribSourcePanel = new RibbonPanelSource();
                ribSourcePanel.Title = PanelName;
                ribSourcePanel.Id = PanelId;
                // теперь саму панель
                RibbonPanel ribPanel = new RibbonPanel();
                ribPanel.Source = ribSourcePanel;
                ribTab.Panels.Add(ribPanel);

                // создаем пустую tooltip (всплывающая подсказка)
                RibbonToolTip tt;

                #region SectionDrawer_RibButton
                // Создаем новый экземпляр подсказки
                tt = new RibbonToolTip();
                // Отключаем вызов справки (в данном примере её нету)
                tt.IsHelpEnabled = false;
                // Создаем кнопку
                RibbonButton SectionDrawer_RibButton = new RibbonButton();
                SectionDrawer_RibButton.CommandParameter = tt.Command = "_ррр";
                SectionDrawer_RibButton.Name = "SectionDrawer";
                SectionDrawer_RibButton.Text = tt.Title = "Построение разреза";
                SectionDrawer_RibButton.CommandHandler = new RibbonCommandHandler();
                SectionDrawer_RibButton.Orientation = System.Windows.Controls.Orientation.Horizontal;
                SectionDrawer_RibButton.Size = RibbonItemSize.Large;

                SectionDrawer_RibButton.LargeImage = LoadImage("Image_32");
                SectionDrawer_RibButton.ShowImage = true;
                SectionDrawer_RibButton.ShowText = true;
                // Заполняем содержимое всплывающей подсказки
                tt.Content = "Вызывает окно ввода данных для отрисовки разреза";
                // Подключаем подсказку к кнопке
                SectionDrawer_RibButton.ToolTip = tt;
                // Добавляем кнопку в RibbonSplitButton
                ribSourcePanel.Items.Add(SectionDrawer_RibButton);
                #endregion
                #region WellPlanarImport_RibButton
                // Создаем новый экземпляр подсказки
                tt = new RibbonToolTip();
                // Отключаем вызов справки (в данном примере её нету)
                tt.IsHelpEnabled = false;
                // Создаем кнопку
                RibbonButton WellPlanarImport_RibButton = new RibbonButton();
                WellPlanarImport_RibButton.CommandParameter = tt.Command = "_ИмпортСкважин";
                WellPlanarImport_RibButton.Name = "WellPlanarImport";
                WellPlanarImport_RibButton.Text = tt.Title = "Импорт скважин";
                WellPlanarImport_RibButton.CommandHandler = new RibbonCommandHandler();
                WellPlanarImport_RibButton.Orientation = System.Windows.Controls.Orientation.Horizontal;
                WellPlanarImport_RibButton.Size = RibbonItemSize.Large;

                WellPlanarImport_RibButton.LargeImage = LoadImage("Image_32");
                WellPlanarImport_RibButton.ShowImage = true;
                WellPlanarImport_RibButton.ShowText = true;
                // Заполняем содержимое всплывающей подсказки
                tt.Content = "Вызывает окно ввода данных для импорта скважин на план";
                // Подключаем подсказку к кнопке
                WellPlanarImport_RibButton.ToolTip = tt;
                // Добавляем кнопку в RibbonSplitButton
                ribSourcePanel.Items.Add(WellPlanarImport_RibButton);
                #endregion

            }
            catch (System.Exception ex)
            {
                Autodesk.AutoCAD.ApplicationServices.Application.
                  DocumentManager.MdiActiveDocument.Editor.WriteMessage(ex.Message);
            }
        }

        System.Windows.Media.Imaging.BitmapImage LoadImage(string ImageName)
        {
            return new System.Windows.Media.Imaging.BitmapImage(
                new Uri("pack://application:,,,/WellCalculations2010;component/" + ImageName + ".png"));
        }
        
        
        
        /* Собственный обраотчик команд
         * Это один из вариантов вызова команды по нажатию кнопки
         */
        class RibbonCommandHandler : System.Windows.Input.ICommand
        {
            public bool CanExecute(object parameter)
            {
                return true;
            }
            public event EventHandler CanExecuteChanged;
            public void Execute(object parameter)
            {
                Document doc = Application.DocumentManager.MdiActiveDocument;
                if (parameter is RibbonButton)
                {
                    // Просто берем команду, записанную в CommandParameter кнопки
                    // и выпоняем её используя функцию SendStringToExecute
                    RibbonButton button = parameter as RibbonButton;
                    Application.DocumentManager.MdiActiveDocument.SendStringToExecute(
                        button.CommandParameter + " ", true, false, true);
                }
            }
        }
    }
}
