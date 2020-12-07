using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using System;

namespace Third
{
    class Program
    {
        const string IncorrectMoveMesssage = "Incorrect move key. Move key must be integer number from the available moves." +
            " Try again.";
        const string ExecExceptionMessage = "Incorrect input parameters. \n " +
            "Parameters must be uniq strings.Their count must be odd and equals at least 3. \n" +
            "Exampple: dotnet thrid.exe rock paper scissors lizard Spock";
        private static Dictionary<int, string> _movesDictionary = new Dictionary<int, string>();
        static void Main(string[] args)
        {
            if (!InputsAreValid(args))
            {
                Console.WriteLine(ExecExceptionMessage);
                return;
            }
            InitializeMovesDictionary(args);

            var aiMove = GetAIMove();
            Console.WriteLine($"HMAC: \n {aiMove.HMACStringView}");

            Console.WriteLine("Available moves:");
            foreach (var mov in _movesDictionary)
                Console.WriteLine($"{mov.Key} : {mov.Value}");

            int plrMoveKey = GetPlayerMove();
            if (plrMoveKey == 0)
                return;

            Console.WriteLine($"Your move: {_movesDictionary[plrMoveKey]}");
            Console.WriteLine($"Computer move: {_movesDictionary[aiMove.MoveKey]}");

            int res = CalcResult(plrMoveKey, aiMove.MoveKey);
            switch (res)
            {
                case 0:
                    Console.WriteLine("Draw!");
                    break;
                case 1:
                    Console.WriteLine("You win!");
                    break;
                case 2:
                    Console.WriteLine("You lose!");
                    break;
            }
            Console.WriteLine($"HMAC key: {aiMove.HMACKeyStringView}");
            Test2(aiMove);


        }

        static void Test()
        {
            for (var i = 1; i < _movesDictionary.Count; i++)
            {
                for (var j = 1; j < _movesDictionary.Count; j++)
                {
                    Console.WriteLine($"Ai {_movesDictionary[i]} Plr {_movesDictionary[j]} Res {CalcResult(i, j)}");
                }
            }
        }

        static void Test2(EncryptedMove move)
        {
            Console.WriteLine();
            using (var hmac = new HMACSHA256(move.HMACKey)){
                var hs = hmac.ComputeHash(Encoding.ASCII.GetBytes(_movesDictionary[move.MoveKey]));
                for (var i = 0; i < hs.Length; i++)
                {
                    if(hs[i]==move.HMAC[i])
                    Console.Write("1");
                    else
                    Console.Write("0");
                }
            }
        }
        static bool InputsAreValid(string[] args)
        {
            if (args.Length < 3 || args.Length % 2 == 0)
                return false;

            for (int i = 0; i < args.Length; i++)
            {
                for (int j = i + 1; j < args.Length; j++)
                {
                    if (args[i] == args[j])
                        return false;
                }
            }

            return true;
        }

        static void InitializeMovesDictionary(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                _movesDictionary.Add(i + 1, args[i]);
            }
            _movesDictionary.Add(0, "exit");
        }

        static EncryptedMove GetAIMove()
        {
            Random rnd = new Random();
            int moveKey = rnd.Next(1, _movesDictionary.Count - 1);

            return new EncryptedMove(moveKey, _movesDictionary);
        }

        static int GetPlayerMove()
        {
            while (true)
            {
                if (int.TryParse(Console.ReadLine(), out int moveKey) && _movesDictionary.ContainsKey(moveKey))
                    return moveKey;
                else
                    Console.WriteLine(IncorrectMoveMesssage);
            }
        }

        static int CalcResult(int playersMoveKey, int aiMoveKey)
        {
            if (playersMoveKey == aiMoveKey)
                return 0;

            float diff = aiMoveKey - playersMoveKey;
            float offset = (_movesDictionary.Count - 1) / 2.0f;

            if ((diff > 0 && diff > offset) || (diff < 0 && -diff < offset))
                return 1;
            else
                return 2;
        }
    }

    internal class EncryptedMove
    {
        internal readonly int MoveKey;
        internal readonly byte[] HMACKey;
        internal readonly byte[] HMAC;

        internal string HMACStringView
        {
            get
            {
                return BitConverter.ToString(HMAC).Replace("-", "");
            }
        }
 
        internal string HMACKeyStringView
        {
            get
            {
                return BitConverter.ToString(HMACKey).Replace("-", "");
            }
        }

        internal EncryptedMove(int moveKey, Dictionary<int, string> movesDictionary)
        {
            MoveKey = moveKey;
            HMACKey = new byte[16];

            using (var cRnd = new RNGCryptoServiceProvider())
            {
                cRnd.GetBytes(HMACKey);
            }

            using (var hmac = new HMACSHA256(HMACKey))
            {
                string aiMove = movesDictionary[moveKey];
                HMAC = hmac.ComputeHash(Encoding.ASCII.GetBytes(aiMove));
            }
        }
    }
}
