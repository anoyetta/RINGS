using System.Collections.Generic;
using System.Linq;

namespace aframe
{
    public static class CompanyNameConverter
    {
        #region 会社称号の定義テーブル

        private static readonly IReadOnlyList<(string Formal, string Short)> CompanySubtitles = new List<(string Formal, string Short)>()
        {
            // 株式会社
            ("株式会社", "(株)"),
            ("株式会社", "㈱"),
            ("株式会社", "(株"),
            ("株式会社", "株)"),
            ("株式会社", "(カ)"),
            ("株式会社", "(カ"),
            ("株式会社", "カ)"),

            // 有限会社
            ("有限会社", "(有)"),
            ("有限会社", "㈲"),
            ("有限会社", "(有"),
            ("有限会社", "有)"),
            ("有限会社", "(ユ)"),
            ("有限会社", "(ユ"),
            ("有限会社", "ユ)"),

            // 医療法人
            ("医療法人", "(医)"),
            ("医療法人", "(医"),
            ("医療法人", "医)"),
            ("医療法人", "(イ)"),
            ("医療法人", "(イ"),
            ("医療法人", "イ)"),
            ("医療法人社団", "(医)"),
            ("医療法人社団", "(医"),
            ("医療法人社団", "医)"),
            ("医療法人社団", "(イ)"),
            ("医療法人社団", "(イ"),
            ("医療法人社団", "イ)"),
            ("医療法人財団", "(医)"),
            ("医療法人財団", "(医"),
            ("医療法人財団", "医)"),
            ("医療法人財団", "(イ)"),
            ("医療法人財団", "(イ"),
            ("医療法人財団", "イ)"),
            ("社会医療法人", "(医)"),
            ("社会医療法人", "(医"),
            ("社会医療法人", "医)"),
            ("社会医療法人", "(イ)"),
            ("社会医療法人", "(イ"),
            ("社会医療法人", "イ)"),

            // 福祉系
            ("宗教法人", "(宗)"),
            ("宗教法人", "宗)"),
            ("宗教法人", "(宗"),
            ("宗教法人", "(シユウ)"),
            ("宗教法人", "シユウ)"),
            ("宗教法人", "(シユウ"),
            ("学校法人", "(学)"),
            ("学校法人", "学)"),
            ("学校法人", "(学"),
            ("学校法人", "(ガク)"),
            ("学校法人", "ガク)"),
            ("学校法人", "(ガク"),
            ("社会福祉法人", "(福)"),
            ("社会福祉法人", "福)"),
            ("社会福祉法人", "(福"),
            ("社会福祉法人", "(フク)"),
            ("社会福祉法人", "フク)"),
            ("社会福祉法人", "(フク"),

            // 士業系
            ("税理士法人", "(税)"),
            ("税理士法人", "税)"),
            ("税理士法人", "(税"),
            ("税理士法人", "(ゼイ)"),
            ("税理士法人", "ゼイ)"),
            ("税理士法人", "(ゼイ"),
            ("弁護士法人", "(弁)"),
            ("弁護士法人", "弁)"),
            ("弁護士法人", "(弁"),
            ("弁護士法人", "(ベン)"),
            ("弁護士法人", "ベン)"),
            ("弁護士法人", "(ベン"),
            ("行政書士法人", "(行)"),
            ("行政書士法人", "行)"),
            ("行政書士法人", "(行"),
            ("行政書士法人", "(ギヨ)"),
            ("行政書士法人", "ギヨ)"),
            ("行政書士法人", "(ギヨ"),
            ("司法書士法人", "(司)"),
            ("司法書士法人", "司)"),
            ("司法書士法人", "(司"),
            ("司法書士法人", "(シホウ)"),
            ("司法書士法人", "シホウ)"),
            ("司法書士法人", "(シホウ"),
            ("社会保険労務士法人", "(労)"),
            ("社会保険労務士法人", "労)"),
            ("社会保険労務士法人", "(労"),
            ("社会保険労務士法人", "(ロウム)"),
            ("社会保険労務士法人", "ロウム)"),
            ("社会保険労務士法人", "(ロウム"),
        };

        #endregion 会社称号の定義テーブル

        /// <summary>
        /// 会社名から称号を除去する
        /// </summary>
        /// <param name="companyName"></param>
        /// <returns></returns>
        public static string Omit(
            string companyName)
        {
            var result = companyName;

            // 一旦全角に変換する
            result = result.ToWide();

            foreach (var entry in CompanySubtitles)
            {
                result = result.Replace(entry.Formal, string.Empty);
                result = result.Replace(entry.Short.ToWide(), string.Empty);
            }

            result = result.Trim();

            return result;
        }

        /// <summary>
        /// 会社名の称号を正式表記にする
        /// </summary>
        /// <param name="companyName"></param>
        /// <returns></returns>
        public static string ToFormal(
            string companyName)
        {
            var result = companyName;

            // 一旦全角に変換する
            result = result.ToWide();

            foreach (var entry in CompanySubtitles)
            {
                result = result.Replace(entry.Short.ToWide(), entry.Formal);
            }

            result = result.Trim();

            return result;
        }

        /// <summary>
        /// 会社名の称号を省略表記にする
        /// </summary>
        /// <param name="companyName"></param>
        /// <returns></returns>
        public static string ToShort(
            string companyName)
        {
            var result = companyName;

            // 一旦正式表記に変換する
            result = ToFormal(result);

            var topSubtitles = CompanySubtitles
                .GroupBy(x => x.Formal)
                .Select(x => x.First());

            foreach (var entry in topSubtitles)
            {
                result = result.Replace(entry.Formal, entry.Short.ToNarrow());
            }

            result = result.Trim();

            return result;
        }
    }
}
