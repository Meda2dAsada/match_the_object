# **VR Project - Match the Object**

## **How the Code Works**

In this project, we created a simple VR game in Unity where the player needs to grab an object and place it in the correct target.

The main idea is to compare the object with the target to check if they match.

### **Object Comparison**

1. The player grabs an object using the VR controllers.  
2. The player moves the object to a target.  
3. When the object enters the target area, the code checks if they match.  
4. If the object is correct, the game accepts it.  
5. If the object is incorrect, the player needs to try again.

### **How the Comparison Works in Code**

We use C\# scripts in Unity to manage the comparison. Each object and target can have an identifier.

When an object enters the target area, the script compares both identifiers using a simple condition:

```
if (objectID == targetID)
{
    // Correct object
}
else
{
    // Incorrect object
}
```

This allows the game to recognize whether the player placed the correct object in the correct location.

## **How AI Helped Us**

We used AI to help us understand how to create this mechanic in Unity.

AI explained how to detect when an object enters a target area and how to compare the objects using C\#.


With these explanations, we were able to understand the logic better and implement the matching system in our VR project.

## **Conclusion**

This activity helped us understand how object interactions work in VR and how Unity can compare objects using simple C\# conditions. AI was useful because it explained the process step by step and made the code easier to understand.

