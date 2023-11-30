using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Class that takes care of all the drawing of trees, does not get instantiated itself
/// </summary>
public class Turtle
{
    public delegate void TreeSegment(Vector3 start, Vector3 end);
    private TreeSegment BranchDraw;


    public Vector3 Position;
    public Quaternion Rotation;

    // If transform stack is empty, use this transform info
    private TransformInfo lastTransformInfo;

    // For pushing and popping (saving / loading) transforms
    public class TransformInfo
    {
        public Vector3 Position;
        public Quaternion Rotation;

        public TransformInfo(Vector3 position, Quaternion rotation)
        {
            Position = position;
            Rotation = rotation;
        }
    }

    // Keep track of all the current transform infos
    private Stack<TransformInfo> transformStack = new Stack<TransformInfo>();


    public Turtle(Vector3 position, TreeSegment branch)
    {
        Position = position;
        BranchDraw = branch;
        lastTransformInfo = new TransformInfo(Position, Rotation);
    }

    // All the rules this turtle can use in drawing 
    #region RULES

    // Moves the turtle forward and draws a branch
    public void Translate(Vector3 delta)
    {
        delta = Rotation * delta;
        BranchDraw(Position, Position + delta);
        Position += delta;
    }

    // Rotates the turtle given an angle change
    public void Rotate(Vector3 delta) => Rotation = Quaternion.Euler(Rotation.eulerAngles + delta);

    // Saving transform info
    public void Push()
    {
        TransformInfo transformInfo = new TransformInfo(Position, Rotation);
        transformStack.Push(transformInfo);

        // saving the newest transform info
        lastTransformInfo = transformInfo;
    }

    // Getting the saved transform info
    public void Pop()
    {
        try
        {
            var poppedTransfrom = transformStack.Pop();
            Position = poppedTransfrom.Position;
            Rotation = poppedTransfrom.Rotation;
        }

        // If there are no transforms in the stack, use the newest one
        catch (System.InvalidOperationException)
        {
            Position = lastTransformInfo.Position;
            Rotation = lastTransformInfo.Rotation;

        }
    }
    #endregion
}
