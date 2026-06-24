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
    public Vector2 RandomVelocity;
    public float isLife;

    float randomX;
    float randomY;

    void Start()
    {
        randomX = Random.Range(-RandomVelocity.x, RandomVelocity.x);
        randomY = Random.Range(RandomVelocity.y, RandomVelocity.y * 2);
    }

    void Update()
    {
        this.transform.position += new Vector3(randomX, randomY, 0.0f) * Time.deltaTime;

        Destroy(this.gameObject, isLife);
    }
}