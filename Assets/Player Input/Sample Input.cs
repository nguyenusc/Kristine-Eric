//------------------------------------------------------------------------------
// <auto-generated>
//     This code was auto-generated by com.unity.inputsystem:InputActionCodeGenerator
//     version 1.11.2
//     from Assets/Player Input/Sample Input.inputactions
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public partial class @SampleInput: IInputActionCollection2, IDisposable
{
    public InputActionAsset asset { get; }
    public @SampleInput()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""Sample Input"",
    ""maps"": [
        {
            ""name"": ""Test Player"",
            ""id"": ""55abe843-7170-46f5-ac39-74b444ea1215"",
            ""actions"": [
                {
                    ""name"": ""Move"",
                    ""type"": ""Value"",
                    ""id"": ""f9d4957e-9e0a-4a3d-85b4-b18d407a6d6d"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                }
            ],
            ""bindings"": [
                {
                    ""name"": ""2D Vector"",
                    ""id"": ""353fb638-6b91-4d14-b95a-e9ddfbb87abb"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""a6f5bf85-5c8b-4abc-91a8-fde104fb49c3"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""7702eff9-89db-4799-b7bf-38eb2aa7c8a2"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""11301ce5-0a15-45fa-b868-5d1a506174cc"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""1dd0a2f2-23a7-469d-b426-a97f74e8914e"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
        // Test Player
        m_TestPlayer = asset.FindActionMap("Test Player", throwIfNotFound: true);
        m_TestPlayer_Move = m_TestPlayer.FindAction("Move", throwIfNotFound: true);
    }

    ~@SampleInput()
    {
        UnityEngine.Debug.Assert(!m_TestPlayer.enabled, "This will cause a leak and performance issues, SampleInput.TestPlayer.Disable() has not been called.");
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }

    public IEnumerable<InputBinding> bindings => asset.bindings;

    public InputAction FindAction(string actionNameOrId, bool throwIfNotFound = false)
    {
        return asset.FindAction(actionNameOrId, throwIfNotFound);
    }

    public int FindBinding(InputBinding bindingMask, out InputAction action)
    {
        return asset.FindBinding(bindingMask, out action);
    }

    // Test Player
    private readonly InputActionMap m_TestPlayer;
    private List<ITestPlayerActions> m_TestPlayerActionsCallbackInterfaces = new List<ITestPlayerActions>();
    private readonly InputAction m_TestPlayer_Move;
    public struct TestPlayerActions
    {
        private @SampleInput m_Wrapper;
        public TestPlayerActions(@SampleInput wrapper) { m_Wrapper = wrapper; }
        public InputAction @Move => m_Wrapper.m_TestPlayer_Move;
        public InputActionMap Get() { return m_Wrapper.m_TestPlayer; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(TestPlayerActions set) { return set.Get(); }
        public void AddCallbacks(ITestPlayerActions instance)
        {
            if (instance == null || m_Wrapper.m_TestPlayerActionsCallbackInterfaces.Contains(instance)) return;
            m_Wrapper.m_TestPlayerActionsCallbackInterfaces.Add(instance);
            @Move.started += instance.OnMove;
            @Move.performed += instance.OnMove;
            @Move.canceled += instance.OnMove;
        }

        private void UnregisterCallbacks(ITestPlayerActions instance)
        {
            @Move.started -= instance.OnMove;
            @Move.performed -= instance.OnMove;
            @Move.canceled -= instance.OnMove;
        }

        public void RemoveCallbacks(ITestPlayerActions instance)
        {
            if (m_Wrapper.m_TestPlayerActionsCallbackInterfaces.Remove(instance))
                UnregisterCallbacks(instance);
        }

        public void SetCallbacks(ITestPlayerActions instance)
        {
            foreach (var item in m_Wrapper.m_TestPlayerActionsCallbackInterfaces)
                UnregisterCallbacks(item);
            m_Wrapper.m_TestPlayerActionsCallbackInterfaces.Clear();
            AddCallbacks(instance);
        }
    }
    public TestPlayerActions @TestPlayer => new TestPlayerActions(this);
    public interface ITestPlayerActions
    {
        void OnMove(InputAction.CallbackContext context);
    }
}
