using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InteractiveMenu : MonoBehaviour
{
    public abstract void OnValueChanged();
    public abstract void Show();
    public virtual void Hide()
    {
        if (IteractiveController.Instance)
        {
            IteractiveController.Instance.Hide();
        }
    }

}
