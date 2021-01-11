using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Runtime;
using Android.Widget;
using System;

namespace GoFishClient
{
    // This is the top bar
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {

        Button connectButton;
        Button ButtonGoFish;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

            //Init Components
            connectButton = FindViewById<Button>(Resource.Id.ButtonConnect);
            ButtonGoFish = FindViewById<Button>(Resource.Id.ButtonGoFish);

            // Connect to server
            connectButton.Click += (sender, e) =>
            {
                connectButton.Text = "Sæd";
                connectButton.Enabled = false;
                ButtonGoFish.Enabled = true;



            };
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

    }
}