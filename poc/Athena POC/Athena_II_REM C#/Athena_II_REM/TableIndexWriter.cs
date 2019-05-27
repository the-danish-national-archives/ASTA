using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Athena_II_REM
{	
    /// <summary>
    /// Klasse der udskriver en tableIndex.xml fil
	/// </summary>
    public static class TableIndexWriter
    {  
        /// <summary>
        /// Starter en ny tableIndex med xml deklaration og root element
        /// </summary>
        /// <param name="xw">XML writerinstans</param>
        /// <param name="dbName">databasenavn</param>
        /// <param name="databaseProduct">databaseprodukt</param>
        public static void writeStartXML(XmlTextWriter xw, string dbName, string databaseProduct)
        {	
            xw.WriteStartDocument();
            xw.WriteStartElement("siardDiark");
            
            xw.WriteAttributeString("xsi:schemaLocation", "http://www.sa.dk/xmlns/diark/1.0 ../Schemas/standard/tableIndex.xsd");
            xw.WriteAttributeString("xmlns", "http://www.sa.dk/xmlns/diark/1.0");
            xw.WriteAttributeString("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance");
 
            xw.WriteElementString("version", "1.0");
         xw.WriteElementString("dbName", "ikke_relevant");
         xw.WriteElementString("databaseProduct", "Microsoft SQL server");
            xw.WriteStartElement("tables");// husk at afslutte            
        }
      
        
        /// <summary>
        /// Skriver starten på en tabel
        /// </summary>
        /// <param name="xw">XML writerinstans</param>
        /// <param name="tableName">tabellens fysiske navn</param>
        /// <param name="folderNum">foldernummer</param>
        /// <param name="description">tabelbeskrivelse</param>
        public static void writeTableIndex(XmlTextWriter xw, string tableName, string folderNum, string description)
        {
        	xw.WriteStartElement("table");// husk at afslutte
        	xw.WriteElementString ( "name", tableName );
            xw.WriteElementString ( "folder", folderNum );
            xw.WriteElementString ("description",description);
            xw.WriteStartElement("columns");// husk at afslutte
        }
        	
	
        /// <summary>
		/// Metode der udskriver et sæt af kolonneparametre
		/// </summary>
		/// <param name="xw">XmlWriter instans</param>
		/// <param name="columnName">kolonnenavn</param>
		/// <param name="columnID">kolonnenummer</param>
		/// <param name="type">datatype</param>
		/// <param name="typeOriginal">den originale datatype samme som datattype</param>
		/// <param name="nullable">Kan kolonnen være tom</param>
		/// <param name="description">beskrivelse af kolonne</param>
        public static void writeColumn ( XmlTextWriter xw, string columnName, string columnID, string type, string typeOriginal, string nullable, string description )
        {
            xw.WriteStartElement ( "column" );
            xw.WriteElementString ( "name", columnName );
            xw.WriteElementString ( "columnID", columnID );
            xw.WriteElementString ( "type", type );
            xw.WriteElementString ( "typeOriginal", typeOriginal );
            xw.WriteElementString ( "nullable", nullable );
            xw.WriteElementString ( "description", description );
            xw.WriteEndElement ( ); //column
        }
        /// <summary>
        /// Afslutter et sæt af kolonner i en tabel
        /// </summary>
        /// <param name="xw">XmlWriter instans</param>
        public static  void writeEndColumns ( XmlTextWriter xw )
        {
            xw.WriteEndElement ( ); //columns
        }
   
    	
        /// <summary>
        /// Metode til at skrive en tabels primærnøgle
        /// </summary>
        /// <param name="xw">XmlWriter instans</param>
        /// <param name="pkName">Navn på primærnøgle</param>
        /// <param name="columnsPk">Primærnøglens kolonner</param>
        public static void writePrimaryKey ( XmlTextWriter xw, string pkName, List<string> columnsPk )
        {
            xw.WriteStartElement("primaryKey");
            xw.WriteElementString ("name", pkName );
            foreach (string pkColumn in columnsPk)
            {
                xw.WriteElementString ("column",pkColumn);
            }
            xw.WriteEndElement(); //primaryKey
        }


       /// <summary>
       /// Metode til at  starte en række af fremmednøgler
       /// </summary>
       /// <param name="xw">XmlWriter instans</param>
        public static  void writeForeignKeysStart ( XmlTextWriter xw )
        {
            xw.WriteStartElement ( "foreignKeys" );
        }
        
      
        /// <summary>
        /// Metode til at skrive fremmednøgler
        /// </summary>
        /// <param name="xw">XmlWriter instans</param>
        /// <param name="fkName">fremmednøglenavn</param>
        /// <param name="referencedTable">Tabel der refereres til</param>
        /// <param name="columnsFK">Kolonner i fremmednøgle </param>
        /// <param name="columnsReferenced">kolkolonner der refereres til</param>
        public static void writeForeignKey ( XmlTextWriter xw, string fkName, string referencedTable, string columnsFK, string columnsReferenced)
        {
            if ( true) // der skal kun sættes fremmednøgler ind hvis der findes nogen
            {
            	xw.WriteStartElement ( "foreignKey" );
	            xw.WriteElementString ( "name", fkName );
	            xw.WriteElementString ( "referencedTable", referencedTable );	           
	            xw.WriteStartElement("reference");
	            xw.WriteElementString("column",columnsFK);
	            xw.WriteElementString("referenced", columnsReferenced);
	            xw.WriteEndElement ( );	
	       	    xw.WriteEndElement ( );//foreignKey	            
            }
        }


        /// <summary>
        /// Afsluter en fremmednøgle
        /// </summary>
        /// <param name="xw">XmlWriter instans</param>
        public static void writeForeignKeysEnd(XmlTextWriter xw)
        {
            xw.WriteEndElement();
        }
     
        /// <summary>
        /// Afslutter en tabele i tableIndex
        /// </summary>
        /// <param name="xw">XmlWriter instans</param>
        /// <param name="rows"></param>
        public static  void writeEndTable ( XmlTextWriter xw, string rows )
        {
            xw.WriteElementString ( "rows", rows );
            xw.WriteEndElement ( ); //table
        }
        
        /// <summary>
        /// Afslutter tableIndex
        /// </summary>
        /// <param name="xw">XmlWriter instans</param>
        public static void writeEndDocument ( XmlTextWriter xw )
        {
            xw.WriteEndElement ( );// tables
            xw.WriteEndElement ( );// rootelement
            xw.Flush ( );
            xw.Close ( );
        }
    }
}
