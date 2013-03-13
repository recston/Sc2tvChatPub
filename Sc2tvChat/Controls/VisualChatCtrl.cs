﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace RatChat.Controls {
    [TemplatePart(Name = "PART_Messages", Type = typeof(ListBox))]
    [TemplatePart(Name = "PART_OptionsButton", Type = typeof(Button))]
    [TemplatePart(Name = "PART_CloseButton", Type = typeof(Button))]
    [TemplatePart(Name = "PART_Header", Type = typeof(Label))]
    public class VisualChatCtrl : UserControl, INotifyPropertyChanged {
        static VisualChatCtrl() {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(VisualChatCtrl),
                new FrameworkPropertyMetadata(typeof(VisualChatCtrl)));
        }

        public VisualChatCtrl()
            : base() {
            this.SetResourceReference(VisualChatCtrl.StyleProperty, "VisualChatStyle");
            ChatDataSource = new ObservableCollection<VisualMessage>();
        }

        ListBox PART_Messages;
        Button PART_OptionsButton, PART_CloseButton;
        Label PART_Header;


        protected void FireChange( string PropertyName ) {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(PropertyName));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        ObservableCollection<VisualMessage> ChatDataSource;
        RatChat.Core.IChatSource _Source;
        public RatChat.Core.IChatSource Source {
            get { return _Source; }
            private set {
                _Source = value;
              //  this.DataContext = _Source;
                FireChange("Source");
            }
        }
        public ChatSourceManager Manager { get; set; }

        //public string VisualId { get; set; }
        //public string SourceChatId { get; set; }

        public void ConnectToChatSource( RatChat.Core.IChatSource Source ) {
            this.Source = Source;
            this.Source.OnNewMessagesArrived += Source_OnNewMessagesArrived;
            this.Source.BeginWork();
        }

        void Source_OnNewMessagesArrived( List<Core.ChatMessage> NewMessages ) {

            if (this.Dispatcher.CheckAccess()) {
                Safe_Source_OnNewMessagesArrived(NewMessages);
            } else {
                this.Dispatcher.BeginInvoke(new Action(() => {
                    Safe_Source_OnNewMessagesArrived(NewMessages);
                }));
            }
        }

        void Safe_Source_OnNewMessagesArrived( List<Core.ChatMessage> NewMessages ) {
            for (int j = 0; j < NewMessages.Count; ++j) {
                VisualMessage vm = new VisualMessage(Source.StreamerNick, Manager.SmilesDataDase, NewMessages[j]);
                if (ChatDataSource.Count > 0)
                    vm.DoubleName = vm.Data.Name == ChatDataSource[ChatDataSource.Count - 1].Data.Name;
                ChatDataSource.Add(vm);
            }

            if (ChatDataSource.Count > 100) {
                while (ChatDataSource.Count > 40)
                    ChatDataSource.RemoveAt(0);
            }
        }

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();

            this.PART_Messages = this.GetTemplateChild("PART_Messages") as ListBox;
            this.PART_CloseButton = this.GetTemplateChild("PART_CloseButton") as Button;
            this.PART_OptionsButton = this.GetTemplateChild("PART_OptionsButton") as Button;
            this.PART_Header = this.GetTemplateChild("PART_Header") as Label;

            this.PART_CloseButton.Click += PART_CloseButton_Click;
            this.PART_OptionsButton.Click += PART_OptionsButton_Click;

            this.PART_Messages.DataContext = ChatDataSource;
            this.PART_Header.DataContext = Source;
        }

        void PART_OptionsButton_Click( object sender, RoutedEventArgs e ) {
            if (ChatOptionsWindow.ShowOptionsWindow(this, Manager.ChatConfigStorage)) {
                Source.OnConfigApply(Manager.ChatConfigStorage);
                ChatDataSource.Clear();
            }
        }

        void PART_CloseButton_Click( object sender, RoutedEventArgs e ) {
            this.Manager.OnChatClosed(this);
        }

    }
}
