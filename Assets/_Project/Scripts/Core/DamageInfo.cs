using System;
using UnityEngine;

namespace DungeonSurvivor.Core
{

    [Serializable]
    public struct DamageInfo
    {
        [SerializeField] private int _amount;
        [SerializeField] private Vector3 _hitPoint;
        [SerializeField] private Vector3 _hitDirection;
        [SerializeField] private GameObject _instigator;

        public int Amount => _amount;
        public Vector3 HitPoint => _hitPoint;
        public Vector3 HitDirection => _hitDirection;
        public GameObject Instigator => _instigator;

        public DamageInfo(int amount, Vector3 hitPoint, Vector3 hitDirection, GameObject instigator)
        {
            _amount = amount;
            _hitPoint = hitPoint;
            _hitDirection = hitDirection;
            _instigator = instigator;
        }
    }
}

