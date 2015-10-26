using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RailML___WPF
{
    public abstract class BaseViewModel : INotifyPropertyChanged, IDisposable
{
    protected BaseViewModel()
    {
    }

    ~BaseViewModel()
    {
        string msg = string.Format("{0} ({1}) ({2}) Finalized",
            GetType().Name, DisplayName, GetHashCode());
        Debug.WriteLine(msg);
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public virtual string DisplayName
    {
        get; protected set;
    }

    protected virtual bool ThrowOnInvalidPropertyName
    {
        get; private set;
    }

    public void Dispose()
    {
        OnDispose();
    }

    [Conditional("DEBUG")]
    [DebuggerStepThrough]
    public void VerifyPropertyName(string propertyName)
    {
        // Verify that the property name matches a real,
        // public, instance property on this object.
        if (TypeDescriptor.GetProperties(this)[propertyName] == null)
        {
            string msg = "Invalid property name: " + propertyName;

            if (ThrowOnInvalidPropertyName)
            {
                throw new Exception(msg);
            }

            Debug.Fail(msg);
        }
    }

    protected virtual void OnDispose()
    {
    }

    protected virtual void OnPropertyChanged(string propertyName)
    {
        VerifyPropertyName(propertyName);

        PropertyChangedEventHandler handler = PropertyChanged;
        if (handler == null)
        {
            return;
        }

        var e = new PropertyChangedEventArgs(propertyName);
        handler(this, e);
    }
}
}
