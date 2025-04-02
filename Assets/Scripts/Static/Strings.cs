using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Static
{
    public class Strings : MonoBehaviour
    {
        public static string PLAYERNAME = "PlayerName";

        private static string[] SESSIONNAMES =
        {
            "숨바꼭질 챔피언",
            "보일 듯 말 듯 챔피언",
            "숨바꼭질 마스터 클래스",
            "내가 안 보이나?",
            "나를 눕혀봐...",
            "찾기 전에 반성해라",
            "이쯤 되면 포기해라",
            "이 게임은 원래 그런 거야",
            "눈앞에 있어도 못 본다",
            "승자의 법칙"
        };
        
        public static string GenerateRandomProfileName()
        {
            return "User_" + Guid.NewGuid().ToString("N")[..8];
        }
        
        public static string GenerateRandomSessionId()
        {
            return "Session_" + Guid.NewGuid().ToString("N")[..8];
        }
        
        public static string GetRandomSessionName()
        {
            return SESSIONNAMES[Random.Range(0, SESSIONNAMES.Length)];
        }
    }
}
