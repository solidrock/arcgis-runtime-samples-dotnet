// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGISRuntime.Samples.Managers;
using Esri.ArcGISRuntime.LocalServices;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Drawing;

namespace ArcGISRuntime.WPF.Samples.DynamicWorkspaceShapefile
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Dynamic workspace shapefile",
        "Local Server",
        "This sample demonstrates how to dynamically add a local shapefile to a map using Local Server.",
        "This sample depends on the local server being installed and configured. See https://developers.arcgis.com/net/latest/wpf/guide/local-server.htm for details and instructions.\nClick on the 'Choose Shapefile' button to select a shapefile. The file picker will start in the sample viewer's offline data directory. Sample shapefiles are loaded in the background. ")]
	[ArcGISRuntime.Samples.Shared.Attributes.OfflineData("ea619b4f0f8f4d108c5b87e90c1b5be0", "d98b3e5293834c5f852f13c569930caa")]
    public partial class DynamicWorkspaceShapefile
    {
        // Hold a reference to the local map service
        private LocalMapService _localMapService;

        // Hold a reference to the layer that will display the shapefile data
        private ArcGISMapImageSublayer _shapefileSublayer;

        public DynamicWorkspaceShapefile()
        {
            InitializeComponent();

            // Create the UI, setup the control references and execute initialization
            Initialize();
        }

        private async void Initialize()
        {
            // Create a map and add it to the view
            MyMapView.Map = new Map(BasemapType.Topographic, 39.7294, -104.8319, 12);

            try
            {
                // Set the local data path - must be done before starting. On most systems, this will be C:\Esri_LS_Data.
                // This path should be kept short to avoid Windows path length limitations.
                string tempDataPathRoot = Directory.GetParent(Environment.GetFolderPath(Environment.SpecialFolder.Windows)).FullName;
                string tempDataPath = Path.Combine(tempDataPathRoot, "Esri_LS_Data");
                Directory.CreateDirectory(tempDataPath); // CreateDirectory won't overwrite if it already exists.
                LocalServer.Instance.AppDataPath = tempDataPath;

                // Start the local server instance
                await LocalServer.Instance.StartAsync();

                // Load the sample data
                LoadShapefilePaths();

                // Enable the 'choose shapefile' button
                MyChooseButton.IsEnabled = true;
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(String.Format("Please ensure that local server is installed prior to using the sample. See instructions in readme.md or metadata.json. Message: {0}", ex.Message), "Local Server failed to start");
            }
        }

        private async void StartLocalMapService(string filename, string path)
        {
            // Start a service from the blank MPK
            string mapServiceUrl = GetMpkPath();

            // Create the local map service
            _localMapService = new LocalMapService(mapServiceUrl);

            // Create the shapefile workspace
            ShapefileWorkspace shapefileWorkspace = new ShapefileWorkspace("shp_wkspc", path);

            // Create the layer source that represents the shapefile on disk
            TableSublayerSource source = new TableSublayerSource(shapefileWorkspace.Id, filename);

            // Create a sublayer instance from the table source
            _shapefileSublayer = new ArcGISMapImageSublayer(0, source);

            // Add the dynamic workspace to the map service
            _localMapService.SetDynamicWorkspaces(new List<DynamicWorkspace>() { shapefileWorkspace });

            // Subscribe to notifications about service status changes
            _localMapService.StatusChanged += _localMapService_StatusChanged;

            try
            {
                // Start the map service
                await _localMapService.StartAsync();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Error");
            }
        }

        private async void _localMapService_StatusChanged(object sender, StatusChangedEventArgs e)
        {
            // Add the shapefile layer to the map once the service finishes starting
            if (e.Status == LocalServerStatus.Started)
            {
                // Create the imagery layer
                ArcGISMapImageLayer imageryLayer = new ArcGISMapImageLayer(_localMapService.Url);

                // Subscribe to image layer load status change events
                // Only set up the sublayer renderer for the shapefile after the parent layer has finished loading
                imageryLayer.LoadStatusChanged += (q, ex) =>
                {
                    // Add the layer to the map once loaded
                    if (ex.Status == Esri.ArcGISRuntime.LoadStatus.Loaded)
                    {
                        // Create a default symbol style
                        SimpleLineSymbol lineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Color.Red, 3);

                        // Apply the symbol style with a renderer
                        _shapefileSublayer.Renderer = new SimpleRenderer(lineSymbol);

                        // Add the shapefile sublayer to the imagery layer
                        imageryLayer.Sublayers.Add(_shapefileSublayer);
                    }
                };

                try
                {
                    // Load the layer
                    await imageryLayer.LoadAsync();

                    // Clear any existing layers
                    MyMapView.Map.OperationalLayers.Clear();

                    // Add the image layer to the map
                    MyMapView.Map.OperationalLayers.Add(imageryLayer);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), "Error");
                }
            }
        }

        private static string GetMpkPath()
        {
            // Gets the path to the blank map package
            return DataManager.GetDataFolder("ea619b4f0f8f4d108c5b87e90c1b5be0", "mpk_blank.mpk");
        }

        private static string LoadShapefilePaths()
        {
            // Gets the path to the shapefile package
            return DataManager.GetDataFolder("d98b3e5293834c5f852f13c569930caa", "TrailBikeNetwork.shp");
        }

        private void MyChooseButton_Click(object sender, RoutedEventArgs e)
        {
            // Allow the user to specify a file path - create the dialog
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog()
            {
                DefaultExt = ".shp",
                Filter = "Shapefiles|*.shp",
                InitialDirectory = Path.GetDirectoryName(LoadShapefilePaths())
            };

            // Show the dialog and get the results
            bool? result = dlg.ShowDialog();

            // Take action if the user selected a file
            if (result == true)
            {
                string filename = Path.GetFileName(dlg.FileName);
                string path = Path.GetDirectoryName(dlg.FileName);
                StartLocalMapService(filename, path);
            }
        }
    }
}