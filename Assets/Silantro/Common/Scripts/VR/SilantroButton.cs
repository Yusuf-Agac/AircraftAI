using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Oyedoyin;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

/// <summary>
/// Handles the functioning of VR Buttons
/// </summary>
/// /// <remarks>
/// This component will marks pressable VR buttons that can be used to call functions on the aircraft
/// or its components
/// </remarks>

public class SilantroButton : MonoBehaviour
{
    // ------------------------------ Selectibles
    public enum CurrentState { ButtonOn, ButtonOff }
    /// <summary>
    /// Current button state either On(pressed) or Off(unpressed)
    /// </summary>
    public CurrentState state = CurrentState.ButtonOn;
    public enum ButtonType { SingleAction, DoubleAction }
    /// <summary>
    /// Single action button call a function once when pressed. Double action buttons call a function
    /// when press down and call another function when pressed again to release
    /// </summary>
    public ButtonType buttonType = ButtonType.DoubleAction;
    public enum PressAxis { X, Y, Z }
    /// <summary>
    /// What axis will the button move when pressed?
    /// </summary>
    public PressAxis pressAxis = PressAxis.Z;
    public enum PressDirection { Inwards, Outwards }
    /// <summary>
    /// The button press direction
    /// </summary>
    public PressDirection pressDirection = PressDirection.Inwards;
    public enum ButtonAction { Press, Flip }
    /// <summary>
    /// Is the button pressed normally or flipped like a switch
    /// </summary>
    public ButtonAction buttonAction = ButtonAction.Press;
    public enum FlipDirection { CW, CCW }
    public FlipDirection flipDirection = FlipDirection.CW;


    // ---------------------------- Events
    [Serializable] public class InteractionEvent : UnityEvent { }
    /// <summary>
    /// Event called when the double action button is pressed on
    /// </summary>
    public InteractionEvent onPressOn = new InteractionEvent();
    /// <summary>
    /// Event called when the double action button is released
    /// </summary>
    public InteractionEvent onPressOff = new InteractionEvent();
    /// <summary>
    /// Event called when the single action button is pressed
    /// </summary>
    public InteractionEvent onPress = new InteractionEvent();

    public InteractionEvent onFlipOn = new InteractionEvent();
    public InteractionEvent onFlipOff = new InteractionEvent();

    // ---------------------------- Variables
    private Vector3 initialPosition, targetPosition, pressedPosition, rotationAxis;
    private Quaternion initialRotation, targetRotation;
    private float actualFlipAmount = 0, targetFlipAmount;
    private AudioSource buttonSource;
    private float actualPressDistance = 0;

    public AudioClip clickSound;
    public float pressDistance = 100f;
    public float flipAmount = 20f;
    public float coolDownTime = 4f;
    public float coolTimer; 



    // -----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    private void Start()
    {
        if (clickSound) { Oyedoyin.Common.Misc.Handler.SetupSoundSource(this.transform, clickSound, "Sound Point", 50f, true, false, out buttonSource); buttonSource.volume = 1f; }

        // --------------- Set Initial Variables
        if (buttonAction == ButtonAction.Press)
        {
            initialPosition = transform.localPosition;
            state = CurrentState.ButtonOff;
            targetPosition = initialPosition;
            if (pressDirection == PressDirection.Outwards) { actualPressDistance = -1f * pressDistance; }
            if (pressDirection == PressDirection.Inwards) { actualPressDistance = pressDistance; }

            if (pressAxis == PressAxis.Z) { pressedPosition = initialPosition + transform.forward * actualPressDistance / 10000f; }
            if (pressAxis == PressAxis.Y) { pressedPosition = initialPosition + transform.up * actualPressDistance / 10000f; }
            if (pressAxis == PressAxis.X) { pressedPosition = initialPosition + transform.right * actualPressDistance / 10000f; }
        }

        if (buttonAction == ButtonAction.Flip)
        {
            initialRotation = transform.localRotation; TurnKnobOff();
            if (flipDirection == FlipDirection.CCW) { actualFlipAmount = -1f * flipAmount; }
            if (flipDirection == FlipDirection.CW) { actualFlipAmount = flipAmount; }

            if (pressAxis == PressAxis.Z) { rotationAxis = new Vector3(1, 0, 0); }
            if (pressAxis == PressAxis.Y) { rotationAxis = new Vector3(0, 1, 0); }
            if (pressAxis == PressAxis.X) { rotationAxis = new Vector3(0, 0, 1); }
        }
    }






    // -------------------------------------- Button Action
    void Update()
    {
        if (buttonAction == ButtonAction.Flip)
        {
            this.transform.localRotation = initialRotation; this.transform.Rotate(rotationAxis, targetFlipAmount);
        }
        if (buttonAction == ButtonAction.Press)
        {
            if (this.transform.localPosition != targetPosition) { this.transform.localPosition = Vector3.MoveTowards(this.transform.localPosition, targetPosition, Time.deltaTime * 3); }
        }
        coolTimer += Time.deltaTime;
    }



    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    void OnMouseEnter()
    {
        //Cursor.SetCursor(buttonCursor, offset, CursorMode.Auto);
    }

    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    void OnMouseExit()
    {
        //  Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }


    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    public void TurnButtonOff()
    {
        //BUTTON STATE
        state = CurrentState.ButtonOff;

        //BUTTON ACTION
        if (buttonType == ButtonType.DoubleAction) { targetPosition = initialPosition; onPressOff.Invoke(); }
        if (buttonType == ButtonType.SingleAction) { StartCoroutine(SpringButtonPosition()); }
    }



    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    public void TurnButtonOn()
    {
        //BUTTON STATE
        state = CurrentState.ButtonOn;

        //BUTTON ACTION
        targetPosition = pressedPosition;
        if (buttonType == ButtonType.DoubleAction) { targetPosition = pressedPosition; onPressOn.Invoke(); }
        if (buttonType == ButtonType.SingleAction) { StartCoroutine(SpringButtonPosition()); }
    }


    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    IEnumerator SpringButtonPosition()
    {
        if (buttonType == ButtonType.SingleAction) { onPress.Invoke(); }
        targetPosition = pressedPosition;
        yield return new WaitForSeconds(0.2f);
        targetPosition = initialPosition;
    }


    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    public void ToggleButton()
    {
        if (state == CurrentState.ButtonOn) { TurnButtonOff(); }
        else { TurnButtonOn(); }
    }


    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    public void Press()
    {
        buttonSource.PlayOneShot(clickSound);
    }








    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    public void TurnKnobOff()
    {
        //KNOB STATE
        state = CurrentState.ButtonOff;

        //KNOB ACTION
        targetFlipAmount = 0;
        onFlipOff.Invoke();
    }



    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    public void TurnKnobOn()
    {
        //KNOB STATE
        state = CurrentState.ButtonOn;

        //KNOB ACTION
        targetFlipAmount = actualFlipAmount;
        onFlipOn.Invoke();
    }


    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    public void Flip()
    {
        buttonSource.PlayOneShot(clickSound);
    }


    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    public void ToggleKnob()
    {
        if (state == CurrentState.ButtonOn) { TurnKnobOff(); }
        else { TurnKnobOn(); }
    }
}


#if UNITY_EDITOR
[CanEditMultipleObjects]
[CustomEditor(typeof(SilantroButton))]
public class SilantroButtonEditor : Editor
{
    Color backgroundColor;
    Color silantroColor = new Color(1, 0.4f, 0);
    SilantroButton button;
    private void OnEnable() { button = (SilantroButton)target; }


    public override void OnInspectorGUI()
    {
        backgroundColor = GUI.backgroundColor;
        //DrawDefaultInspector ();
        serializedObject.UpdateIfRequiredOrScript();

        GUI.color = silantroColor;
        EditorGUILayout.HelpBox("Button Config", MessageType.None);
        GUI.color = backgroundColor;
        GUILayout.Space(2f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("buttonAction"), new GUIContent("Action"));
        if (button.buttonAction == SilantroButton.ButtonAction.Press)
        {
            GUILayout.Space(2f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("buttonType"), new GUIContent("Type"));
        }
        GUILayout.Space(2f);
        EditorGUILayout.LabelField("State", button.state.ToString());



        if (button.buttonAction == SilantroButton.ButtonAction.Press)
        {
            GUILayout.Space(10f);
            GUI.color = silantroColor;
            EditorGUILayout.HelpBox("Press Settings", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(2f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("pressAxis"), new GUIContent("Press Axis"));
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("pressDirection"), new GUIContent("Press Direction"));
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("pressDistance"), new GUIContent("Press Distance"));
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("clickSound"), new GUIContent("Press Sound"));
        }
        if (button.buttonAction == SilantroButton.ButtonAction.Flip)
        {
            GUILayout.Space(10f);
            GUI.color = silantroColor;
            EditorGUILayout.HelpBox("Flip Settings", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(2f);
            GUILayout.Space(2f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("pressAxis"), new GUIContent("Press Axis"));
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("flipDirection"), new GUIContent("Flip Distance"));
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("flipAmount"), new GUIContent("Flip Amount"));
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("clickSound"), new GUIContent("Flip Sound"));
        }

        GUILayout.Space(5f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("coolDownTime"), new GUIContent("Press Timer"));


        GUILayout.Space(10f);
        GUI.color = silantroColor;
        EditorGUILayout.HelpBox("Call Events", MessageType.None);
        GUI.color = backgroundColor;
        GUILayout.Space(2f);
        if (button.buttonAction == SilantroButton.ButtonAction.Press)
        {
            if (button.buttonType == SilantroButton.ButtonType.DoubleAction)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("onPressOn"), new GUIContent("Pressed On Call Function"));
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("onPressOff"), new GUIContent("Pressed Off Call Function"));
            }
            else
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("onPress"), new GUIContent("Call Function"));
            }
        }
        if (button.buttonAction == SilantroButton.ButtonAction.Flip)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("onFlipOn"), new GUIContent("Flipped On Call Function"));
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("onFlipOff"), new GUIContent("Flipped Off Call Function"));
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
