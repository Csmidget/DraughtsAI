using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Human : AI {
    
    public override PlayerType GetType()
    {
        return PlayerType.Human;
    }
}
