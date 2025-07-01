using System;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Utils
{
    public static class Util
    {
        public static readonly string PLAYERNAME = "에러가 발생했습니다.";
        public static readonly string PASSWORD = "Password";
        public static readonly string PLAYERSLOT = "PlayerSlot";
        
        private static readonly string[] SESSIONNAMES =
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
        
        public static string GetRandomString(int length)
        {
            return Guid.NewGuid().ToString("N")[..length];
        }
        
        public static string GetRandomSessionName()
        {
            return SESSIONNAMES[Random.Range(0, SESSIONNAMES.Length)];
        }
        
        public static Vector3 GetRandomPositionInSphere(float radius)
        {
            return Random.onUnitSphere.normalized * radius;
        }
    }
}