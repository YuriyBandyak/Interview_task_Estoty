using UnityEngine;

public abstract class GenericFactory<T> : MonoBehaviour {

    public abstract T GetNew();
}
