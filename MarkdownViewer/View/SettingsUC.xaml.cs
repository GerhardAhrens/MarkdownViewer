namespace MarkdownViewer.View
{
    using MarkdownViewer.Core;

    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// Interaktionslogik für SettingsUC.xaml
    /// </summary>
    public partial class SettingsUC : UserControlBase
    {
        public SettingsUC()
        {
            this.InitializeComponent();
            WeakEventManager<UserControl, RoutedEventArgs>.AddHandler(this, "Loaded", this.OnLoaded);
            this.DataContext = this;
        }

        #region Properties
        public string WindowTitel
        {
            get => base.GetValue<string>();
            set => base.SetValue(value);
        }

        public bool SelectionExitAnswer
        {
            get => base.GetValue<bool>();
            set => base.SetValue(value, this.SetBoolSettingHandler);
        }

        public string FileTypText
        {
            get => base.GetValue<string>();
            set => base.SetValue(value);
        }

        public bool SelectionFileTyp
        {
            get => base.GetValue<bool>();
            set => base.SetValue(value, this.SetBoolSettingHandler);
        }

        private ApplicationSettings Settings { get; set; }

        #endregion Properties


        #region WindowEventHandler
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this) == false)
            {
                this.WindowTitel = LocalizationValue.Get("WindowsTitelZeile");

                this.Settings = App.Settings;
                this.SelectionExitAnswer = this.Settings.FrageExit;

                if (FileAssociationManager.IsOwnedByApplication() == false)
                {
                    this.FileTypText = "Der Dateityp *.md ist nicht registriert. Soll der Typ registriert werden?";
                    this.SelectionFileTyp = false;
                }
                else
                {
                    this.FileTypText = "Der Dateityp *.md ist registriert. Soll die registrierung aufgehoben werden?";
                    this.SelectionFileTyp = false;
                }
            }
        }
        #endregion WindowEventHandler

        private void SetBoolSettingHandler(bool arg1, string arg2)
        {
            if (arg2 == nameof(this.SelectionExitAnswer))
            {
                App.Settings.FrageExit = arg1;
            }
            else if (arg2 == nameof(this.SelectionFileTyp))
            {
                if (arg1 == true)
                {
                    if (FileAssociationManager.IsOwnedByApplication() == false)
                    {
                        FileAssociationManager.Register();
                    }
                    else 
                    {
                        FileAssociationManager.Unregister();
                    }
                }
            }
        }

    }
}
