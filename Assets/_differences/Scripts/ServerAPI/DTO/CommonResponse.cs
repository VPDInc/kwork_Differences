using System;

namespace _differences.Scripts.ServerAPI.DTO
{
    [Serializable]
    public struct CommonResponse
    {
        public bool success;
        public string message;
        public PlayerState playerStateUpdate;
    }
}
