
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Athena_II_REM
{	
    
    /// <summary>
	/// Klasse der udskriver en researchIndex.xml fil
	/// </summary>
    public static class ResearchIndexWriter
    {  
        /// <summary>
        /// Starter en ny tableIndex med xml deklaration og root element
        /// </summary>
        /// <param name="xw">XML writerinstans</param>
        /// <param name="dbName">databasenavn</param>
        /// <param name="databaseProduct">databaseprodukt</param>
        public static void writeStartXML(XmlTextWriter xw)
        {	
            xw.WriteStartDocument();
            xw.WriteStartElement("researchIndex");

            xw.WriteAttributeString("xsi:schemaLocation", "http://www.sa.dk/xmlns/diark/1.0 ../Schemas/standard/researchIndex.xsd");
            xw.WriteAttributeString("xmlns", "http://www.sa.dk/xmlns/diark/1.0");
            xw.WriteAttributeString("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance");

            xw.WriteStartElement("mainTables");// husk at afslutte            
        }



        /// <summary>
        /// Skriver starten på en tabel
        /// </summary>
        /// <param name="xw">XML writerinstans</param>
        /// <param name="tableName">tabellens fysiske navn</param>
        /// <param name="folderNum">foldernummer</param>
        /// <param name="description">tabelbeskrivelse</param>
        public static void writeTableIndex(XmlTextWriter xw, string tableID, string source, Boolean specialNumeric)
        {
            xw.WriteStartElement("table");// husk at afslutte
            xw.WriteElementString("tableID", tableID);
            xw.WriteElementString("source", source);
            xw.WriteElementString("specialNumeric", specialNumeric.ToString());
        }
        	

        public static void writeStartColumns(XmlTextWriter xw)
        {
            xw.WriteStartElement("columns");
        }
        	

        public static void writeStartColumn(XmlTextWriter xw, string columnID)
        {
            xw.WriteStartElement("column");
            xw.WriteElementString("columnID", columnID);

        }



        public static void writeStartMissingValues(XmlTextWriter xw)
        {
            xw.WriteStartElement("missingValues");

        }


        public static void writeStartValue(XmlTextWriter xw , string value)
        {
            xw.WriteElementString("value", value);

        }


        public static void writeEndElement(XmlTextWriter xw)
        {
            xw.WriteEndElement(); //columns
        }



        
        /// <summary>
        /// Afslutter tableIndex
        /// </summary>
        /// <param name="xw">XmlWriter instans</param>
        public static void writeEndDocument ( XmlTextWriter xw )
        {

            xw.WriteEndElement ( );// mainTables
            xw.WriteEndElement ( );// researchIndex
            //xw.Flush ( );
            //xw.Close ( );
        }
    }
}
