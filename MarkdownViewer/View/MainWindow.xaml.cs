//-----------------------------------------------------------------------
// <copyright file="MainWindow.cs" company="Lifeprojects.de">
//     Class: MainWindow
//     Copyright © Lifeprojects.de 2026
// </copyright>
//
// <author>Gerhard Ahrens - Lifeprojects.de</author>
// <email>developer@lifeprojects.de</email>
// <date>23.06.2026</date>
//
// <summary>
// WPF Anwendung zur Darstellung von Markdown-Dateien (*.md)
// </summary>
//-----------------------------------------------------------------------

namespace MarkdownViewer
{
    using System.ComponentModel;
    using System.Windows;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : WindowBase
    {
        public MainWindow()
        {
            this.InitializeComponent();
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            this.ResizeMode = ResizeMode.CanResizeWithGrip;
            this.ShowInTaskbar = true;
            this.MinWidth = 400;
            this.MinHeight = 300;

            WeakEventManager<WindowBase, RoutedEventArgs>.AddHandler(this, "Loaded", this.OnLoaded);
            WeakEventManager<WindowBase, CancelEventArgs>.AddHandler(this, "Closing", this.OnWindowClosing);
            this.SetVectorIcon("IconApplicationLogo", 64);

            this.QuitCommand = new CommandBase(() => this.OnQuit("Argument"));
            this.InformationCommand = new CommandBase(this.OnInformationPopup);
            this.SettingsCommand = new CommandBase(this.OnSettingsPopup);
            this.CloseInformationPopupCommand = new CommandBase(this.OnCloseInformation);
            this.CloseSettingsPopupCommand = new CommandBase(this.OnCloseSettingsPopup);

            this.WindowTitel = LocalizationValue.Get("WindowsTitelZeile");

            this.DataContext = this;
        }

        #region Properties
        public CommandBase QuitCommand { get; private set; }
        public CommandBase InformationCommand { get; private set; }
        public CommandBase SettingsCommand { get; private set; }
        public CommandBase CloseInformationPopupCommand { get; private set; }
        public CommandBase CloseSettingsPopupCommand { get; private set; }

        public string WindowTitel
        {
            get => base.GetValue<string>();
            set => base.SetValue(value);
        }

        private MessageBase Message { get; } = new MessageBase();
        #endregion Properties

        #region Windows Events
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            StatusbarMain.Statusbar.DatabaseInfo = "Keine";
            StatusbarMain.Statusbar.DatabaseInfoTooltip = "Keine Datenbank verbunden";
            StatusbarMain.Statusbar.Notification = "Bereit";
        }

        private void OnCloseApplication(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void OnWindowClosing(object sender, CancelEventArgs e)
        {
            e.Cancel = false;

            if (App.Settings.FrageExit == false)
            {
                App.ApplicationExit();
                return;
            }

            MessageBoxResult msgYN;
            if (this.Tag != null)
            {
                msgYN = this.Message.AppExitMessage(this.Tag.ToString());
            }
            else
            {
                msgYN = this.Message.AppExitMessage();
            }

            if (msgYN == MessageBoxResult.Yes)
            {
                App.ApplicationExit();
            }
            else
            {
                e.Cancel = true;
            }
        }

        #endregion Windows Events

        #region Command Handler
        private void OnQuit(string param)
        {
            this.Tag = param;
            this.Close();
        }

        private void OnInformationPopup()
        {
            this.InformationPopup.SetValue(MaskLayerBehavior.IsOpenProperty, true);
        }

        private void OnCloseInformation()
        {
            this.InformationPopup.SetValue(MaskLayerBehavior.IsOpenProperty, false);
        }

        private void OnSettingsPopup()
        {
            this.SettingsPopup.SetValue(MaskLayerBehavior.IsOpenProperty, true);
        }

        private void OnCloseSettingsPopup()
        {
            this.SettingsPopup.SetValue(MaskLayerBehavior.IsOpenProperty, false);
        }
        #endregion
    }
}