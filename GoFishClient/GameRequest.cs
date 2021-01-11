using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace GoFishClient
{
    class GameRequest
    {
        private byte requestType;
        public byte RequestType
        {
            get { return requestType; }
            set { requestType = value; }
        }

        private string fromIP;
        public string FromIP
        {
            get { return fromIP; }
            set { fromIP = value; }
        }

        private string userFrom;
        public string UserFrom
        {
            get { return userFrom; }
            set { userFrom = value; }
        }

        private string userTo;
        public string UserTo
        {
            get { return userTo; }
            set { userTo = value; }
        }

        private byte cardValue;
        public byte CardValue
        {
            get { return cardValue; }
            set { cardValue = value; }
        }

        private List<Card> cardList = new List<Card>();
        public List<Card> CardList
        {
            get { return cardList; }
            set { cardList = value; }
        }

    }
}