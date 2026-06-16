using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace RevitVisibilityDiagnostic_R24.UI
{
    public partial class DiagnosticWindow : Window
    {
        private Document _doc;
        private ExternalEvent _exEvent;
        private DiagnosticEventHandler _handler;
        private DispatcherTimer _timer;

        public DiagnosticWindow(Document doc, ExternalEvent exEvent, DiagnosticEventHandler handler)
        {
            InitializeComponent();
            _doc = doc;
            _exEvent = exEvent;
            _handler = handler;

            // Setup callback
            _handler.OnResultReady = OnResultReady;

            // Load Views into ComboBox
            LoadViews();

            // Setup polling timer (checks every 500ms)
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(500);
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        public class ViewItem
        {
            public View RevitView { get; set; }
            public string Name => RevitView.Name;
            public string ViewTypeName
            {
                get
                {
                    if (RevitView == null) return "";
                    ElementId typeId = RevitView.GetTypeId();
                    if (typeId != ElementId.InvalidElementId)
                    {
                        Element typeElem = RevitView.Document.GetElement(typeId);
                        if (typeElem != null)
                        {
                            return typeElem.Name;
                        }
                    }
                    // Fallback to a cleaner enum name if type element is not found
                    string typeStr = RevitView.ViewType.ToString();
                    if (typeStr == "ThreeD") return "3D View";
                    if (typeStr == "EngineeringPlan") return "Structural Plan";
                    if (typeStr == "FloorPlan") return "Floor Plan";
                    if (typeStr == "CeilingPlan") return "Ceiling Plan";
                    return typeStr;
                }
            }
        }

        private void LoadViews()
        {
            FilteredElementCollector collector = new FilteredElementCollector(_doc);
            var views = collector.OfClass(typeof(View))
                                 .Cast<View>()
                                 .Where(v => !v.IsTemplate 
                                          && v.ViewType != ViewType.Internal 
                                          && v.ViewType != ViewType.ProjectBrowser 
                                          && v.ViewType != ViewType.SystemBrowser 
                                          && v.ViewType != ViewType.Schedule 
                                          && v.ViewType != ViewType.Legend 
                                          && v.ViewType != ViewType.DrawingSheet 
                                          && v.ViewType != ViewType.PanelSchedule 
                                          && v.ViewType != ViewType.ColumnSchedule)
                                 .Select(v => new ViewItem { RevitView = v })
                                 .OrderBy(v => v.ViewTypeName)
                                 .ThenBy(v => v.Name)
                                 .ToList();
            
            ViewsComboBox.ItemsSource = views;
            ViewsComboBox.SelectedItem = views.FirstOrDefault(v => v.RevitView.Id == _doc.ActiveView.Id);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            // Instead of comparing selection (which is tricky outside API context),
            // we just raise the event. The handler will check if selection is valid.
            // A more robust way is tracking selection via Idling event, but raising event periodically is a simple start.
            // Actually, raising an external event every 500ms might be too heavy.
            // For this prototype, we'll raise it, and the handler does a quick check.
            _exEvent.Raise();
        }

        private void ViewsComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var selectedItem = ViewsComboBox.SelectedItem as ViewItem;
            _handler.TargetView = selectedItem?.RevitView;
            _exEvent.Raise();
        }

        private void ViewsComboBox_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Up || e.Key == System.Windows.Input.Key.Down || e.Key == System.Windows.Input.Key.Enter || e.Key == System.Windows.Input.Key.Escape)
                return;

            var textBox = e.OriginalSource as System.Windows.Controls.TextBox;
            if (textBox != null)
            {
                string searchText = textBox.Text;
                System.ComponentModel.ICollectionView view = System.Windows.Data.CollectionViewSource.GetDefaultView(ViewsComboBox.ItemsSource);
                
                if (string.IsNullOrEmpty(searchText))
                {
                    view.Filter = null;
                }
                else
                {
                    view.Filter = item =>
                    {
                        var viewItem = item as ViewItem;
                        if (viewItem == null) return false;
                        return viewItem.Name.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0 ||
                               viewItem.ViewTypeName.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0;
                    };
                }
                ViewsComboBox.IsDropDownOpen = true;
            }
        }

        private void CheckButton_Click(object sender, RoutedEventArgs e)
        {
            _exEvent.Raise();
        }

        private void OnResultReady(DiagnosticResultData data)
        {
            Dispatcher.Invoke(() =>
            {
                if (data.Header.Contains("No element selected") || data.Header.Contains("invalid") || data.Header.Contains("Multiple"))
                {
                    StatusTextBlock.Text = "Waiting for selection...";
                    ResultsHeaderTextBlock.Text = data.Header;
                    ResultsItemsControl.ItemsSource = null;
                }
                else
                {
                    StatusTextBlock.Text = "Selection detected.";
                    ResultsHeaderTextBlock.Text = data.Header;
                    ResultsItemsControl.ItemsSource = data.Results;
                }
            });
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (_timer != null)
            {
                _timer.Stop();
            }
        }
    }
}
