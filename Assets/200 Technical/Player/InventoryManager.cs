using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EnhancedFramework.Core;

namespace LudumDare54
{
    public class InventoryManager : EnhancedSingleton<InventoryManager>
    {
        public  List<Multiblock> CurrentInventory;
        
    }
}
