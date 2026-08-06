/*
* ファイル名　ParticleMove.cs
* タイトル　　パーティクル
* 作成者　　　久保木幹太
* 作成日　　　6月22日
* 更新日　　　6月22日
*/

using UnityEngine;

public class ParticleMove : MonoBehaviour
{
    public float isLife;
    public float speed = 10.0f;
    private Vector3 velocity;
    private float phi = 0.0f;
    private float theta = 0.0f;

    void Start()
    {
        phi = Random.Range(0.0f, 1.0f) * Mathf.PI * 2.0f; // 0から360度
        theta = Random.Range(0.0f, 1.0f) * Mathf.PI; // 0~180度

        velocity = new Vector3(
            Mathf.Sin(theta) * Mathf.Cos(phi),
            Mathf.Cos(theta), 
            Mathf.Sin(theta) * Mathf.Sin(phi)
            );

        velocity *= speed;
    }

    void Update()
    {
        velocity.y -= 9.8f * Time.deltaTime;

        this.transform.position += velocity * Time.deltaTime;

        Destroy(this.gameObject, isLife);
    }
}