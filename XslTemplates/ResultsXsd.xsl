<?xml version="1.0"?>
<!-- transform results generated by xsd.exe out of results from an Everything.config run for publishing with RI build -->
<xsl:stylesheet 
  xmlns="" 
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
  xmlns:xs="http://www.w3.org/2001/XMLSchema" 
  xmlns:msdata="urn:schemas-microsoft-com:xml-msdata" version="1.0">
  <xsl:output method="xml" omit-xml-declaration="no" indent="yes" />
  <xsl:template match="@*|node()">
    <xsl:copy>
      <xsl:apply-templates select="@*|node()"/>
    </xsl:copy>
  </xsl:template>
  <xsl:template match="xs:schema[@id=&quot;NewDataSet&quot;]">
    <xsl:element name="xs:schema">
      <xsl:attribute name="id">
        <xsl:text>remoteinstallresultsgroups</xsl:text>
      </xsl:attribute>
      <xsl:apply-templates select="xs:element[@name=&quot;remoteinstallresultsgroups&quot;]"/>
    </xsl:element>
  </xsl:template>
  <xsl:template match="xs:element[@name=&quot;remoteinstallresultsgroups&quot;]">
    <xs:element name="remoteinstallresultsgroups">
      <xsl:apply-templates select="@*|node()"/>
    </xs:element>
    <xsl:element name="xs:simpleType">
      <xsl:attribute name="name">
        <xsl:text>booleantype</xsl:text>
      </xsl:attribute>
      <xs:restriction base="xs:NMTOKEN">
        <xs:enumeration value="True" />
        <xs:enumeration value="False" />
      </xs:restriction>
    </xsl:element>
  </xsl:template>
  <xsl:template match="xs:element[@name=&quot;SuccessfulInstall&quot;]|xs:element[@name=&quot;SuccessfulUnInstall&quot;]">
    <xsl:element name="xs:element">
      <xsl:attribute name="name">
        <xsl:value-of select="@name" />
      </xsl:attribute>
      <xsl:attribute name="type">booleantype</xsl:attribute>
      <xsl:attribute name="minOccurs">0</xsl:attribute>
    </xsl:element>
  </xsl:template>
  <xsl:template match="xs:attribute[@name=&quot;success&quot;]">
    <xsl:element name="xs:attribute">
      <xsl:attribute name="name">
        <xsl:value-of select="@name" />
      </xsl:attribute>
      <xsl:attribute name="type">booleantype</xsl:attribute>
    </xsl:element>
  </xsl:template>
  <xsl:template match="@msdata:*" />
</xsl:stylesheet>