﻿using System;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using CharacterMap.Core;
using CharacterMap.ViewModel;

namespace CharacterMap
{
    public sealed partial class MainPage : Page
    {
        public MainViewModel MainViewModel { get; set; }

        public AppSettings AppSettings { get; set; }

        public MainPage()
        {
            this.InitializeComponent();
            this.MainViewModel = this.DataContext as MainViewModel;
            this.NavigationCacheMode = NavigationCacheMode.Required;

            AppSettings = new AppSettings();

            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;
            Window.Current.SetTitleBar(TitleBar);

            BorderFadeInStoryboard.Completed += async (o, _) =>
            {
                await Task.Delay(1000);
                BorderFadeOutStoryboard.Begin();
            };
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (null != LstFontFamily.Items)
            {
                if (AppSettings.UseDefaultSelection)
                {
                    if (!string.IsNullOrEmpty(AppSettings.DefaultSelectedFontName))
                    {
                        var lastSelectedFont = LstFontFamily.Items.FirstOrDefault(
                        (i =>
                        {
                            var installedFont = i as InstalledFont;
                            return installedFont != null && installedFont.Name == AppSettings.DefaultSelectedFontName;
                        }));

                        if (null != lastSelectedFont)
                        {
                            LstFontFamily.SelectedItem = lastSelectedFont;
                        }
                    }
                }
                else
                {
                    LstFontFamily.SelectedIndex = 0;
                }
            }
        }

        private void BtnCopy_OnClick(object sender, RoutedEventArgs e)
        {
            var character = CharGrid.SelectedItem as Character;
            if (character != null)
                Edi.UWP.Helpers.Utils.CopyToClipBoard(character.Char);
            BorderFadeInStoryboard.Begin();
        }

        private void TxtFontIcon_OnGotFocus(object sender, RoutedEventArgs e)
        {
            TxtFontIcon.SelectAll();
        }

        private void TxtXamlCode_OnGotFocus(object sender, RoutedEventArgs e)
        {
            TxtXamlCode.SelectAll();
        }

        private void SearchBoxUnicode_OnQuerySubmitted(SearchBox sender, SearchBoxQuerySubmittedEventArgs args)
        {
            var unicodeIndex = SearchBoxUnicode.QueryText;
            int intIndex = Utils.ParseHexString(unicodeIndex);

            var ch = MainViewModel.Chars.FirstOrDefault(c => c.UnicodeIndex == intIndex);
            if (null != ch)
            {
                CharGrid.SelectedItem = ch;
            }
        }

        private async void BtnAbout_OnClick(object sender, RoutedEventArgs e)
        {
            await DigAbout.ShowAsync();
        }

        private async void BtnSavePng_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var bitmap = new RenderTargetBitmap();
                await bitmap.RenderAsync(GridRenderTarget);

                IBuffer buffer = await bitmap.GetPixelsAsync();
                var stream = buffer.AsStream();
                var fileName = $"{DateTime.Now:yyyy-MM-dd-HHmmss}";
                await Utils.SaveStreamToImage(PickerLocationId.PicturesLibrary, fileName, stream, bitmap.PixelWidth, bitmap.PixelHeight);
            }
            catch (Exception ex)
            {
                var dig = new MessageDialog($"{ex.Message}", "Failed to Save PNG File.");
                await dig.ShowAsync();
            }
        }

        private void BtnCopyXamlCode_OnClick(object sender, RoutedEventArgs e)
        {
            Edi.UWP.Helpers.Utils.CopyToClipBoard(TxtXamlCode.Text.Trim());
            BorderFadeInStoryboard.Begin();
        }

        private void BtnCopyFontIcon_OnClick(object sender, RoutedEventArgs e)
        {
            Edi.UWP.Helpers.Utils.CopyToClipBoard(TxtFontIcon.Text.Trim());
            BorderFadeInStoryboard.Begin();
        }

        private async void BtnSettings_OnClick(object sender, RoutedEventArgs e)
        {
            await DigSettings.ShowAsync();
        }

        private void BtnSetDefault_OnClick(object sender, RoutedEventArgs e)
        {
            AppSettings.DefaultSelectedFontName = LstFontFamily.SelectedValue as string;
        }
    }
}
