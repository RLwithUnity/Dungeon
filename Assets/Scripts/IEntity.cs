using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.MLAgents
{
    public interface IEntity
    {
        void agentAction();
        void onDamage();
    }
}
