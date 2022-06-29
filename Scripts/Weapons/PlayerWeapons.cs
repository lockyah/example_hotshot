using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeapons : MonoBehaviour
{
    public enum WeaponType { Pistol, Bomb, Launcher, Whip, Joke }
    public WeaponType CurrentWeapon = WeaponType.Pistol;
    public float TimeSinceLastShot = 0;
    public GameObject WeaponEnd; //Bullet spawn point
    [SerializeField] ObjectPooler pool;
    Animator ani;

    private void Start()
    {
        WeaponEnd = GameObject.Find("Player Weapon End");
        ani = GetComponent<Animator>();
    }

    private void Update()
    {
        if(TimeSinceLastShot < 10f)
        {
            TimeSinceLastShot += Time.deltaTime * Time.timeScale;
        }
    }

    public bool WeaponIdle()
    {
        return TimeSinceLastShot > 5f;
    }


    //Single shot weapon
    public void Pistol(ParseInputs.ButtonState button)
    {
        if(button == ParseInputs.ButtonState.Pressed)
        {
            GameObject bullet = pool.RequestPoolItem("Bullet");

            if(bullet != null)
            {
                TimeSinceLastShot = 0f;
                ani.SetTrigger("PrimaryFire");

                bullet.transform.SetPositionAndRotation(WeaponEnd.transform.position, WeaponEnd.transform.rotation);
                bullet.SetActive(true);
            }
            
        }
    }
}
