<?xml version="1.0"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns="http://www.w3.org/TR/html4/strict.dtd">
  <xsl:output method="html"/>
  <xsl:template match="/">
    <head>
      <style type="text/css">
        .ri-sectionheader { background-color:#000066; font-family:arial,helvetica,sans-serif; font-size:10pt; color:#FFFFFF; }
        .ri-summary { font-family:arial,helvetica,sans-serif; font-size:8pt; color:#901090; }
        td.ri-success { background-color: green; color: white; font-weight: bold; text-align: center; }
        td.ri-failure { background-color: red; color: black; font-weight: bold; text-align: center; }
        td.ri-header { text-align: center; font-weight: bold; }
        td.ri-cell { text-align: center; }
        .ri-note { font-size: 8pt; }
        .ri-note.ri-failure { color: red; font-weight: bold; }
        .ri-groups { width: 640px; }
      </style>
    </head>
    <table align="center" cellpadding="2" cellspacing="0" border="0" width="98%">
      <tr>
        <td class="ri-sectionheader" colspan="5">
          &#160;Remote Installs
        </td>
      </tr>
      <tr>
        <td class="ri-summary" colspan="4">
          Total Installs: <xsl:value-of select="count(//remoteinstallresultsgroup)" />
          <xsl:choose>
            <xsl:when test="count(//remoteinstallresultsgroup[@success='True']) &gt; 0">
              , Successful Installs: <xsl:value-of select="count(//remoteinstallresultsgroup[@success='True'])" />
            </xsl:when>
          </xsl:choose>
          <xsl:choose>
            <xsl:when test="count(//remoteinstallresultsgroup[@success='False']) &gt; 0">
              , Failed Installs: <xsl:value-of select="count(//remoteinstallresultsgroup[@success='False'])" />
            </xsl:when>
          </xsl:choose>
        </td>
      </tr>
    </table>
    <xsl:apply-templates select="//remoteinstallresultsgroups"/>
  </xsl:template>
  <xsl:template match="remoteinstallresultsgroups">
    <table class="ri-groups" cellpadding="2" cellspacing="1" border="0">
      <tr>
        <td class="ri-header">Virtual Machine</td>
        <td class="ri-header">Snapshot</td>
        <td class="ri-header">Success</td>
      </tr>
      <xsl:for-each select="remoteinstallresultsgroup">
        <tr>
          <td class="ri-cell">
            <xsl:value-of select="Vm" />
          </td>
          <td class="ri-cell">
            <xsl:value-of select="Snapshot" />
            <div class="ri-note">
              <xsl:value-of select="Description" />
            </div>
          </td>
          <td class="ri-cell">
            <xsl:attribute name="class">
              <xsl:choose>
                <xsl:when test="@success='True'">ri-success</xsl:when>
                <xsl:otherwise>ri-failure</xsl:otherwise>
              </xsl:choose>
            </xsl:attribute>
            <xsl:value-of select="@success" />
          </td>
        </tr>
        <tr>
          <td align="center" colspan="3">
            <xsl:apply-templates select="remoteinstallresults"/>
          </td>
        </tr>
      </xsl:for-each>
    </table>
  </xsl:template>
  <xsl:template match="remoteinstallresults">
    <table class="ri-results">
      <xsl:for-each select="remoteinstallresult">
        <tr>
          <td align="center">
            <div class="ri-note">
              <xsl:value-of select="InstallerName" />
              <xsl:text> </xsl:text>
              <xsl:value-of select="InstallerVersion" />
            </div>
            <div class="ri-note">
              <span class="ri-failure">
                <xsl:value-of select="LastError" />
              </span>
            </div>
          </td>
          <td align="center">
            <xsl:attribute name="class">
              <xsl:choose>
                <xsl:when test="@success='True'">ri-success</xsl:when>
                <xsl:otherwise>ri-failure</xsl:otherwise>
              </xsl:choose>
            </xsl:attribute>
            <div class="ri-note">
              <xsl:value-of select="@success" />
            </div>
          </td>
        </tr>
        <xsl:for-each select="copyfiles/copyfile">
          <xsl:choose>
            <xsl:when test="string-length(Data) &gt; 1">
              <tr>
                <td colspan="2" class="data">
                  <xsl:value-of select="Data" disable-output-escaping="yes" />
                </td>
              </tr>
            </xsl:when>
          </xsl:choose>
        </xsl:for-each>
      </xsl:for-each>
    </table>
  </xsl:template>
</xsl:stylesheet>