using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskAnalysis.Core.Interfaces;

namespace TaskAnalysis.Service.Helpers
{
    public class EmbeddingHelperService : IEmbeddingHelperService
    {
        public EmbeddingHelperService()
        {
        }

        public int GetStableHash(string value)
        {
            unchecked
            /* unchecked
             C#’ta integer taşması (overflow) normalde hata üretir.

             unchecked → taşma olursa hata verme, sayıyı olduğu gibi devam ettir.
             */
            {
                var hash = 23;

                foreach (var c in value)
                    hash = hash * 31 + c;

                return hash;
            }
        }

        public void Normalize(float[] vector) // Uzun metinler ve kısa metinleri karşılaştırabilmek yani vektörün uzunluğunu 1 yapmak için var. 
        { /*Normalize işleminin matematiksel dayanağı aslında vektör normu ve cosine similarity(iki vektörün benzerliği ölçmek için ama biz direkt llm'e veriyoruz) kavramlarına dayanıyor. 
        Normalize işlemi → vektörün uzunluğunu 1 yaparak farklı uzunluktaki metinleri karşılaştırmayı kolaylaştırır.
        Vektörün uzunluğu (magnitude) → √(x1² + x2² + ... + xn²)
        Normalize edilmiş vektör → her bileşen / magnitude
        Bu sayede kısa ve uzun metinler arasındaki benzerlik daha adil şekilde ölçülebilir.

        Neden Yapıyoruz?
Uzun metinlerde daha çok kelime olur → vektörde daha çok “1” birikir → vektörün uzunluğu büyür.

Kısa metinlerde daha az kelime olur → vektörün uzunluğu küçük kalır.

Eğer normalize etmezsen, uzun metinler hep daha “büyük” görünür ve benzerlik karşılaştırması bozulur.
        */
            double sum = 0;

            foreach (var value in vector)
                sum += value * value;

            var magnitude = Math.Sqrt(sum);

            if (magnitude == 0)
                return;

            for (int i = 0; i < vector.Length; i++)
                vector[i] = (float)(vector[i] / magnitude);
        }
    }
}
