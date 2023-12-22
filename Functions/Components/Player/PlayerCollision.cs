using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace RTFunctions.Functions.Components.Player
{
    public class PlayerCollision : MonoBehaviour
    {
        public RTPlayer player;

        void OnTriggerEnter2D(Collider2D other) => player?.OnChildTriggerEnter(other);

        void OnTriggerEnter(Collider other) => player?.OnChildTriggerEnterMesh(other);

        void OnTriggerStay2D(Collider2D other) => player?.OnChildTriggerStay(other);

        void OnTriggerStay(Collider other) => player?.OnChildTriggerStayMesh(other);
    }
}
