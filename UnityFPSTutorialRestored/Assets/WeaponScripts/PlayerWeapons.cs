using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
GENERAL NOTE: These scripts are a direct translation from the original UnityScript .js files.
I've attempted to keep their original functionality as close as possible.
However, there are occasionally some improvements or changes to the scripts which are noted.
If you want to compare the scripts the original JS files are still in the project. (As text asset files since unity has since removed UnityScript long ago)
*/

public class PlayerWeapons : MonoBehaviour
{
    // Start is called before the first frame update
    private void Start()
    {
        // Select the first weapon
        SelectWeapon(0);
    }

    // Update is called once per frame
    private void Update()
    {
        // Did the user press fire?
        if (Input.GetButton("Fire1"))
            BroadcastMessage("Fire");

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SelectWeapon(0);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SelectWeapon(1);
        }
    }

    public void SelectWeapon(int index)
    {
        for (var i = 0; i < transform.childCount; i++)
        {
            // Activate the selected weapon
            if (i == index)
                transform.GetChild(i).gameObject.SetActive(true);
            // Deactivate all other weapons
            else
                transform.GetChild(i).gameObject.SetActive(false);
        }
    }
}
