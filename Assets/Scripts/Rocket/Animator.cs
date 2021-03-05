using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Rocket
{
    public class Animator : MonoBehaviour
    {
        public Animation anim;
        public Animation towerTop;
        public Animation towerBot;

        public ParticleSystem rocketExhaust;
        public ParticleSystem towerSmoke1;
        public ParticleSystem towerSmoke2;

        public Light exhaustLight;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (Keyboard.current[Key.Enter].isPressed)
            {
                rocketExhaust.Play();
                exhaustLight.enabled = true;
                StartCoroutine(PostLaunch());


            }
        }

        IEnumerator PostLaunch()
        {
            yield return new WaitForSeconds(3);

            anim.Play("Launch");
            towerTop.Play("TopRetract");
            towerBot.Play("BotRetract");
            StartCoroutine(InAir());
        }

        IEnumerator InAir()
        {
            yield return new WaitForSeconds(2);

            towerSmoke1.Stop();
            towerSmoke2.Stop();
        }
    }

}
