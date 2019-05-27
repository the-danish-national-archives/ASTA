using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Athena_II_REM
{
    public class DataConverter
    {
        public Boolean isINT(string value)
        {
            int tempint;

            value = value.Replace('.', ',');
            
            //int.TryParse(value, out tempint);
            return (int.TryParse(value, out tempint));
        }


        public Boolean isDEC(string value)
        {
            decimal tempDec;
            //decimal.TryParse(value, out tempDec);
            return (decimal.TryParse(value, out tempDec));
        }

        //*********** Convert to DECIMAL *************
        //(??????)

        //*********** Convert to DATE *************
        private string Convert_DATE(string dateString)
        {
            //!!! egentlig mere en test end en konvertering !!!
            
            dateString = dateString.Trim().Replace('/', '-');

            string[] elementer = dateString.Trim().Split('-');
            if (elementer.Length != 3)
            {
                MessageBox.Show("DATE format fejl! " + dateString);
                return (dateString);//returner det som kom ind!!! (kunne være nil eller "")
            }

            string newDateString = elementer[0] + '-' + elementer[1].PadLeft(2, '0') + '-' + elementer[2].PadLeft(2, '0');

            return (newDateString);
        }

       

        //*********** Convert to TIME *************
        public string Convert_TIME(string timeString)
        {
            //!!! egentlig mere en test end en konvertering !!!
            string[] elementer = timeString.Trim().Split(':');
            if (elementer.Length != 3)
            {
                MessageBox.Show("TIME format fejl! " + timeString);
                return (timeString);//returner det som kom ind!!! (kunne være nil eller "")
            }

            string newTimeString = elementer[0].PadLeft(2,'0') + ':' + elementer[1].PadLeft(2,'0') + ':' + elementer[2].PadLeft(2,'0');

            return (newTimeString);
        }


        //*********** Convert to datetime *************
        public string Convert_ISO8601(string dateTimeString)
        {
            byte output;

            //Fjern alle for og bagved -stillede mellemrum etc.
            dateTimeString = dateTimeString.Trim();

            //Vi udskifter alle ikke numerisk seperatorer med *
            string newDateTimeString = "";
            foreach (char c in dateTimeString)
            {
                if (byte.TryParse(c.ToString(), out output) == true)
                {
                    newDateTimeString = newDateTimeString + c;
                }
                else
                {
                    newDateTimeString = newDateTimeString + '*';
                }
            }

            //Vi splitter med *
            string[] elementer = newDateTimeString.Split('*');

            newDateTimeString = elementer[2] + '-' + elementer[1] + '-' + elementer[0] + 'T' + elementer[3] + ':' + elementer[4] + ':' + elementer[5];
            //MessageBox.Show(dateString + " --> " + newDateString);
            return (newDateTimeString);
        }

        public string Convert_datetime20(string dateTimeString)
        {
            //IBM datetime20 format
            //ddmmccyy:tt:mm:ss  --> XML 2002-05-30T09:30:10
            //fx: 15Jan1960:00:20:34 --> 1960-01-15T00:20:34
            //Opdel ved første ':' skal resulterer i 2 elementer hvoraf det sidste er hh:mm:ss
            int index = dateTimeString.IndexOf(' ');
            string head = dateTimeString.Substring(0, index).Trim();
            string tail = dateTimeString.Replace(head, "").Remove(0, 1);//Dato del samt første ':' fra tidsdel skal fjernes

           // MessageBox.Show(head);
           // MessageBox.Show(tail);

            //head skal parses hvilket bedst sker fra enden af
            string year = head.Substring(head.Length - 4, 4);//altid 4 cifre 
            string monthStr = head.Substring(head.Length - 8, 3);//Jan,Feb,Mar ...
            string day = head.Remove(head.Length - 9);//resten angiver dag i form af 1 eller 2 cifre
           
          //  MessageBox.Show(year);
          //  MessageBox.Show(monthStr);
          //  MessageBox.Show(day);
            
            int month = 0;
            switch (monthStr.ToLower())
            {
                case "jan": month = 1; break;
                case "feb": month = 2; break;
                case "mar": month = 3; break;
                case "apr": month = 4; break;
                case "may": month = 5; break;
                case "jun": month = 6; break;
                case "jul": month = 7; break;
                case "aug": month = 8; break;
                case "sep": month = 9; break;
                case "oct": month = 10; break;
                case "nov": month = 11; break;
                case "dec": month = 12; break;
            }
           // MessageBox.Show(year + '-' + month.ToString().PadLeft(2, '0') + '-' + day.PadLeft(2, '0') + 'T' + tail);
            return (year + '-' + month.ToString().PadLeft(2, '0') + '-' + day.PadLeft(2, '0') + 'T' + tail);
        }




        /// <summary>
        /// Funktion til konvertering af data udfra oprindelig type. En datetime20 konverteres fx. til ISO-6801
        /// </summary>
        /// <param name="dtOrg"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public string Convert_data_to_1007format(string data, string dtOrg)
        {

            //ALT data er af natur STRING og der skal typisk ikke fortages konvertering, med mindre der er tale om datotyper
            try
            {
                dtOrg = dtOrg.Trim();//

                data = data.Trim();
                if (data == "") return ("");

                //*** STRING ***
                if (dtOrg.ToLower() == "string") return (data);//Ingen konvertering
                else if (dtOrg.Contains('%') && dtOrg.Contains('s')) return (data);//her kunne testes for position af '%' + tekst + 's'
                else if (dtOrg.Contains('$') && (isINT(dtOrg.Substring(1, dtOrg.Length - 2)) == true) && dtOrg.Contains('.')) return (data);//SAS datatype $w. - her kunne testes for position af '$' + tekst + '.' 
                //SPSS datatype aw behandles senere når alle andre er afprøvet ...

                //*** INT ***
                else if (dtOrg.ToLower() == "int") return (data);//Ingen konvertering
                else if (dtOrg.Contains('%') && dtOrg.Contains(".0f")) return (data);//her kunne testes for position af '%' + tekst + ".0f" STATA
                else if (dtOrg.Contains('%') && dtOrg.Contains(".0g")) return (data);//her kunne testes for position af '%' + tekst + ".0g" STATA
                else if ((isINT(dtOrg.Substring(dtOrg.Length - 2)) == true) && (dtOrg[dtOrg.Length - 1] == '.')) return (data); //SAS datatype w.
                //SPSS datatype fw behandles senere når alle andre er afprøvet ...

                //*** DEC ***
                else if (dtOrg.ToLower() == "decimal") return (data);//Ingen konvertering, men hvad med evt punktum/komma problematik?
                else if (dtOrg.ToLower() == "dec") return (data);//Ingen konvertering, men hvad med evt punktum/komma problematik?
                else if (dtOrg.Contains('%') && dtOrg.Contains(".") && dtOrg.Contains("f")) return (data);//STATA - her kunne testes for position af '%' + tekst + ".df" Fx %12.6f
                else if (dtOrg.Contains('%') && dtOrg.Contains(".") && dtOrg.Contains("g")) return (data);//STATA - her kunne testes for position af '%' + tekst + ".dg" Fx %13.7g
                else if ((isDEC(dtOrg.Substring(dtOrg.Length - 2)) == true) && (dtOrg.Contains('.'))) return (data); //w.d hvor w er et tal fx 4.2
                else if ((dtOrg[0] == 'f') && (dtOrg.Contains('.'))) return (data); //SPSS fw.d hvor w = tal  fx f65.3

                //*** DATE ***
                else if (dtOrg.ToLower() == "date") return (Convert_DATE(data));
                else if (dtOrg.Contains("%td")) return (Convert_DATE(data));//STATA
                else if (dtOrg.Contains("yymmdd.")) return (Convert_DATE(data));//SAS
                else if (dtOrg.Contains("yymmdd10.")) return (Convert_DATE(data));//SAS
                else if (dtOrg.Contains("sdate10")) return (Convert_DATE(data));//SPSS

                //*** TIME ***
                else if (dtOrg.ToLower() == "time") return (Convert_TIME(data));
                else if (dtOrg.Contains("%tc") && dtOrg.Contains(':')) return (Convert_TIME(data));
                else if (dtOrg.Contains("time.")) return (Convert_TIME(data));
                else if (dtOrg.Contains("time8")) return (Convert_TIME(data));
                else if (dtOrg.Contains("time8.")) return (Convert_TIME(data));

                //*** DATETIME ***
                else if (dtOrg.ToLower() == "datetime") return (data); //Ingen konvertering ... tilsyneladende
                else if (dtOrg.Contains("%tc") && dtOrg.Contains('!')) return (Convert_ISO8601(data));
                else if (dtOrg.Contains("e8601dt.")) return (Convert_ISO8601(data));
                else if (dtOrg.Contains("e8601dt19.")) return (Convert_ISO8601(data));
                else if (dtOrg.Contains("datetime20")) return (Convert_datetime20(data));

                //*** INTEGER skal ske til sidst !!! ***
                else if ((dtOrg[0] == 'f') && (isINT(dtOrg.Substring(1)) == true)) return (data);

                //*** CHAR skal ske til sidst !!! ***
                else if (dtOrg.Contains('a')) return (data);//her kunne testes for position af 'a' + tekst

                //*** Kodelister
         //           isINT ....... !!!!
                //!!!!!Tjek at isINT rent faktisk fungerer efter hensigten !!!!!!
                //todo
                else if ((!isINT(dtOrg.Substring(dtOrg.Length - 2)) == true) && (dtOrg[dtOrg.Length - 1] == '.'))
                {
                    return (data);
                }

            }
            catch
            {
                return ("fejl");//Fejl
            }

            MessageBox.Show("Kunne ikke konvertere data. Datatype:" + dtOrg + "    Data:" + data);
            return ("?");//Ukendt typeangivelse
        }



        
        /// <summary>
        /// Bestemmer hvilken 1007 datatype som der vil blive konverteret til
        /// </summary>
        /// <param name="dataTypeOriginal"></param>
        /// <returns></returns>
        public string get1007datatype(string dataTypeOriginal)
        {
            try
            {
                dataTypeOriginal = dataTypeOriginal.Trim();//
              //  MessageBox.Show(dataTypeOriginal);

                //*** KODELISTER (skal ske her!!!) ***
                if (dataTypeOriginal[0] == '$' && (isINT(dataTypeOriginal.Substring(1, dataTypeOriginal.Length - 2)) == true) && dataTypeOriginal[dataTypeOriginal.Length - 1] == '.') return ("CHAR");//Navn på en kodeliste med foranstillet $ og efterstillet punktum som angiver at den er string
                else if ((dataTypeOriginal[0] != '$') && (!isINT(dataTypeOriginal.Substring(dataTypeOriginal.Length - 2)) == true) && (dataTypeOriginal[dataTypeOriginal.Length - 1] == '.')) return ("INTEGER");//Navn på en kodeliste med efterstillet punktum som angiver at den er nummerisk

                //*** STRING ***
                else if (dataTypeOriginal.ToLower() == "string") return ("CHAR");
                else if (dataTypeOriginal.Contains('%') && dataTypeOriginal.Contains('s')) return ("CHAR");//her kunne testes for position af '%' + tekst + 's'
              

                //*** INT ***
                else if (dataTypeOriginal.ToLower() == "int") return ("INTEGER");
                else if (dataTypeOriginal.Contains('%') && dataTypeOriginal.Contains(".0f")) return ("INTEGER");//her kunne testes for position af '%' + tekst + ".0f"
                else if (dataTypeOriginal.Contains('%') && dataTypeOriginal.Contains(".0g")) return ("INTEGER");//her kunne testes for position af '%' + tekst + ".0g"
                //isINT skal ændres !!!!
                else if ((isINT(dataTypeOriginal.Substring(dataTypeOriginal.Length - 2)) == true) && (dataTypeOriginal[dataTypeOriginal.Length - 1] == '.')) return ("INTEGER");


                //*** DEC ***
                else if (dataTypeOriginal.ToLower() == "decimal") return ("DECIMAL");
                else if (dataTypeOriginal.ToLower() == "dec") return ("DECIMAL");
                else if (dataTypeOriginal.Contains('%') && dataTypeOriginal.Contains(".") && dataTypeOriginal.Contains("f")) return ("DECIMAL");//her kunne testes for position af '%' + tekst + ".df"
                else if (dataTypeOriginal.Contains('%') && dataTypeOriginal.Contains(".") && dataTypeOriginal.Contains("g")) return ("DECIMAL");//her kunne testes for position af '%' + tekst + ".dg"
                else if ((dataTypeOriginal[0] == 'f') && (dataTypeOriginal.Contains("."))) return ("DECIMAL");//fw.d
                    //er de 2 næste ikke identiske?
                    //isINT skal ændres !!!!
                else if ((isINT(dataTypeOriginal.Substring(dataTypeOriginal.Length - 2)) == true) && (dataTypeOriginal.Contains(".d"))) return ("DECIMAL");
                else if (dataTypeOriginal.Contains(".d")) return ("DECIMAL");

                //*** DATE ***
                else if (dataTypeOriginal.ToLower() == "date") return ("DATE");
                else if (dataTypeOriginal.Contains("%td")) return ("DATE");
                else if (dataTypeOriginal.Contains("yymmdd.")) return ("DATE");
                else if (dataTypeOriginal.Contains("yymmdd10.")) return ("DATE");
                else if (dataTypeOriginal.Contains("sdate10")) return ("DATE");

                //*** TIME ***
                else if (dataTypeOriginal.ToLower() == "time") return ("TIME");
                else if (dataTypeOriginal.Contains("%tc") && dataTypeOriginal.Contains(':')) return ("TIME");
                else if (dataTypeOriginal.Contains("time.")) return ("TIME");
                else if (dataTypeOriginal.Contains("time8")) return ("TIME");
                else if (dataTypeOriginal.Contains("time8.")) return ("TIME");

                //*** DATETIME ***
                if (dataTypeOriginal.ToLower() == "datetime") return ("TIMESTAMP");
                else if (dataTypeOriginal.Contains("%tc") && dataTypeOriginal.Contains('!')) return ("TIMESTAMP");
                else if (dataTypeOriginal.Contains("e8601dt.")) return ("TIMESTAMP");
                else if (dataTypeOriginal.Contains("e8601dt19.")) return ("TIMESTAMP");
                else if (dataTypeOriginal.Contains("datetime20")) return ("TIMESTAMP");

                //*** INTEGER skal ske til sidst !!! ***
                else if ((dataTypeOriginal[0] == 'f') && (isINT(dataTypeOriginal.Substring(1)) == true)) return ("INTEGER");
                    
                //*** CHAR skal ske til sidst !!! ***
                else if (dataTypeOriginal[0] == 'a') return ("CHAR");//her kunne testes for position af 'a' + tekst



            }
            catch
            {
                return ("fejl");//Fejl
            }

            MessageBox.Show("Kunne ikke bestemme datatype: " + dataTypeOriginal);
            return ("?");//Ukendt typeangivelse
        }

    }
}
