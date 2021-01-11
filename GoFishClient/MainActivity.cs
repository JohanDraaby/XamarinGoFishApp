using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Runtime;
using Android.Widget;
using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using System.Threading;
using System.Collections.Generic;

namespace GoFishClient
{
    // This is the top bar
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {

        Button connectButton;
        Button buttonGoFish;
        Spinner playerCardSpinner;

        static TcpClient client = new TcpClient();

        static List<Card> hand = new List<Card>();

        static bool active = false;

        static byte points = 0;



        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

            //Init Components
            connectButton = FindViewById<Button>(Resource.Id.ButtonConnect);
            buttonGoFish = FindViewById<Button>(Resource.Id.ButtonGoFish);
            playerCardSpinner = FindViewById<Spinner>(Resource.Id.PlayerCardSpinner);

            // Connect to server
            connectButton.Click += (sender, e) =>
            {
                connectButton.Enabled = false;
                buttonGoFish.Enabled = true;

                IPAddress IP = IPAddress.Parse("172.16.19.10");
                int port = 5000;


                client = new TcpClient();

                client.Connect(IP, port);
                connectButton.Text = "Sæd";

                NetworkStream networkStream = client.GetStream();

                Thread thread = new Thread(o => ReceiveData((TcpClient)o));
                thread.Start(client);

                SendData(client);

                setupSpinner();
            };
        }

        void setupSpinner()
        {
            String[] items = new String[hand.Count];
            // Create a list of items for the spinner
            for (int i = 0; i < hand.Count; i++)
            {
                items[i] = hand[i].FullName;
            }



            //create an adapter to describe how the items are displayed, adapters are used in several places in android.
            //There are multiple variations of this, but this is the basic variant.

            ArrayAdapter adapter = new ArrayAdapter(this, (Resource.Id.PlayerCardSpinner), items);
            //set the spinners adapter to the previously created one.
            playerCardSpinner.Adapter = adapter;
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        private static void ReceiveData(TcpClient client)
        {
            // Receive requests
            NetworkStream ns = client.GetStream();

            byte[] receiveBytes = new byte[1024];
            int byteCount;

            while ((byteCount = ns.Read(receiveBytes, 0, receiveBytes.Length)) > 0)
            {
                GameRequest gr = JsonConvert.DeserializeObject<GameRequest>(Encoding.UTF8.GetString(receiveBytes, 0, byteCount));

                Console.WriteLine("RECEIVED REQUEST TYPE: " + gr.RequestType);

                switch (gr.RequestType)
                {
                    // Answer on request
                    case 1:
                        if (gr.UserTo == "Chris")
                        {
                            Console.WriteLine("It's your turn, boi!");
                        }
                        else
                        {
                            Console.WriteLine("It's not your turn yet!!");
                        }
                        break;
                    // Receive card(s)
                    case 2:
                        Console.WriteLine("=====");
                        Console.WriteLine("Received card(s):");
                        Console.WriteLine(gr.CardValue);
                        Console.WriteLine(gr.CardList[0].FullName);
                        Console.WriteLine("=====");
                        AddCardsToCollection(gr.CardList, gr.UserFrom);
                        CheckForSets();
                        break;
                    case 3:
                        GiveCardsAway(gr.CardValue, client);
                        break;
                    default:
                        break;
                }
            }
        }

        private static void CheckForSets()
        {
            bool setFound = false;

            for (byte i = 1; i < 15; i++)
            {
                byte counter = 0;
                foreach (Card c in hand)
                {
                    if (c.Value == i)
                        counter++;

                    if (counter == 4)
                    {
                        setFound = true;
                        break;
                    }
                }

                if (setFound)
                {
                    points++;

                    Console.WriteLine("================");
                    Console.WriteLine("Found a set!");
                    Console.WriteLine("Current number of sets: " + points);
                    Console.WriteLine("================");

                    for (int j = hand.Count - 1; j >= 0; j--)
                    {
                        if (hand[j].Value == i)
                        {
                            hand.RemoveAt(j);
                        }
                    }

                    if (hand.Count == 0)
                    {
                        GoFish();
                    }

                    setFound = false;
                }
            }
        }

        private static void GoFish()
        {
            Console.WriteLine("============================");
            Console.WriteLine("GO FISH");
            GameRequest gr = new GameRequest();
            gr.UserFrom = "Chris";
            gr.UserTo = "Chris";

            string s;

            NetworkStream ns = client.GetStream();

            gr.RequestType = 3;
            byte[] buffer;

            gr.RequestType = 3;
            gr.CardValue = 0;

            s = JsonConvert.SerializeObject(gr, Formatting.Indented);
            buffer = Encoding.UTF8.GetBytes(s);
            ns.Write(buffer, 0, buffer.Length);
            Console.WriteLine("Sent data to server:");
            Console.WriteLine(s);
            Console.WriteLine("============================");

            Console.WriteLine("Freezing player status..");
            active = false;
        }

        private static void AddCardsToCollection(List<Card> cardlist, string opponentName)
        {
            Console.WriteLine("Add cards to collection.");
            Console.WriteLine("Received:");

            if (cardlist.Count > 0)
            {
                for (int i = 0; i < cardlist.Count; i++)
                {
                    hand.Add(cardlist[i]);
                }
            }

            Console.WriteLine("From " + opponentName);

        }

        private static void GiveCardsAway(byte cardValue, TcpClient client)
        {
            Console.WriteLine("Give cards away.");
            List<Card> cardsToSend = new List<Card>();
            // Check cards
            for (int i = hand.Count - 1; i >= 0; i--)
            {
                if (hand[i].Value == cardValue)
                {
                    cardsToSend.Add(hand[i]);
                    hand.RemoveAt(i);
                }
            }

            if (hand.Count == 0)
            {
                GoFish();
            }

            string s;
            GameRequest gr = new GameRequest();
            NetworkStream ns = client.GetStream();

            gr.CardList = cardsToSend;
            gr.RequestType = 3;
            gr.UserTo = "Kenned";
            gr.UserFrom = "Chris";
            gr.CardValue = cardValue;

            s = JsonConvert.SerializeObject(gr, Formatting.Indented);
            byte[] buffer = Encoding.UTF8.GetBytes(s);
            ns.Write(buffer, 0, buffer.Length);
        }

        private static void SendData(TcpClient client)
        {
            byte requestType;
            int cardValue;
            GameRequest gr = new GameRequest();
            gr.UserFrom = "Chris";
            gr.UserTo = "Kenned";

            string s;

            NetworkStream ns = client.GetStream();

            ConnectionRequest cr = new ConnectionRequest();
            cr.Username = "Chris";
            cr.RequestType = 1;

            gr.RequestType = 1;
            s = JsonConvert.SerializeObject(cr, Formatting.Indented);
            byte[] buffer = Encoding.UTF8.GetBytes(s);
            ns.Write(buffer, 0, buffer.Length);

            //while (!string.IsNullOrEmpty((requestType = Console.ReadLine())))
            while (true)
            {
                Console.WriteLine("1. Show Cards");
                Console.Write("Choose a gameRequest: ");
                requestType = byte.Parse(Console.ReadLine());

                Console.WriteLine("Choose any of these cards:");
                for (int i = 0; i < hand.Count; i++)
                {
                    Console.WriteLine(i + ": " + hand[i].FullName);
                }
                Console.WriteLine();
                Console.WriteLine();
                Console.Write("Insert choice: ");
                cardValue = int.Parse(Console.ReadLine());

                gr.CardValue = hand[cardValue].Value;
                gr.RequestType = 1;
                s = JsonConvert.SerializeObject(gr, Formatting.Indented);
                buffer = Encoding.UTF8.GetBytes(s);
                ns.Write(buffer, 0, buffer.Length);
                Console.WriteLine("Sent data to server:");
                Console.WriteLine(s);
            }
        }

    }
}