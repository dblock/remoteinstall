<?xml version="1.0" encoding="utf-8"?>
<html xsl:version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns="http://www.w3.org/1999/xhtml">
  <head>
    <style type="text/css">
      body, tr { color: #000000; background-color: white; font-family: Verdana; font-size: 10pt; }
      .groups_table { width: 800px; }
      .results_table { width: 600px; }
      td.groups_header { text-align: center; font-weight: bold; }
      td { text-align: center; background-color: #EEEEEE; }
      td.success { background-color: green; color: white; font-weight: bold; }
      td.failure { background-color: red; color: black; font-weight: bold; }
      .note { font-size: 8pt; }
      .note.failure { color: red; font-weight: bold; }
      .data { font-size: 8pt; }
      li { font-weight: bold; color: black; }
      li.success { font-weight: bold; color: green; }
      li.failure { font-weight: bold; color: red; }
      td.successnote { color: green; font-size: 8pt; }
      td.failurenote { color: red; font-size: 8pt; }
      td.errornote { color: red; font-size: 8pt; }
      td.warningnote { color: orange; font-size: 8pt; }
      td.headernote { text-align: center; font-weight: bold; font-size: 8pt; }
    </style>
    <meta http-equiv="Content-Type" content="text/html; charset=UTF-8"/>
  </head>
  <body>
    <ul>
      <li>
        Total Installs: <xsl:value-of select="count(//remoteinstallresultsgroup)" />
      </li>
      <xsl:choose>
        <xsl:when test="count(//remoteinstallresultsgroup[@success='True']) &gt; 0">
          <li class="success">
            Successful Installs: <xsl:value-of select="count(//remoteinstallresultsgroup[@success='True'])" />
          </li>
        </xsl:when>
      </xsl:choose>
      <xsl:choose>
        <xsl:when test="count(//remoteinstallresultsgroup[@success='False']) &gt; 0">
          <li class="failure">
            Failed Installs: <xsl:value-of select="count(//remoteinstallresultsgroup[@success='False'])" />
          </li>
        </xsl:when>
      </xsl:choose>
    </ul>
    <table class="groups_table">
      <tr>
        <td class="groups_header">Virtual Machine</td>
        <td class="groups_header">Snapshot</td>
        <td class="groups_header">Success</td>
      </tr>
      <xsl:for-each select="remoteinstallresultsgroups/remoteinstallresultsgroup">
        <tr>
          <td>
            <xsl:value-of select="Vm" />
          </td>
          <td>
            <xsl:value-of select="Snapshot" />
            <div class="note">
              <xsl:value-of select="Description" />
            </div>
          </td>
          <td>
            <xsl:attribute name="class">
              <xsl:choose>
                <xsl:when test="@success='True'">success</xsl:when>
                <xsl:otherwise>failure</xsl:otherwise>
              </xsl:choose>
            </xsl:attribute>
            <xsl:value-of select="@success" />
          </td>
        </tr>
        <tr>
          <td colspan="3">
            <table class="results_table">
              <xsl:for-each select="remoteinstallresults/remoteinstallresult">
                <tr>
                  <td>
                    <xsl:value-of select="InstallerName" />
                    <xsl:text> </xsl:text>
                    <xsl:value-of select="InstallerVersion" />
                  </td>
                  <td>
                    <xsl:attribute name="class">
                      <xsl:choose>
                        <xsl:when test="@success='True'">success</xsl:when>
                        <xsl:otherwise>failure</xsl:otherwise>
                      </xsl:choose>
                    </xsl:attribute>
                    <xsl:value-of select="@success" />
                  </td>
                </tr>
                <tr>
                  <td colspan="4" class="note">
                    <xsl:choose>
                      <xsl:when test="string-length(InstallLogfile) &gt; 1">
                        <xsl:text>&#187; successful install: </xsl:text>
                        <b>
                          <a>
                            <xsl:attribute name="href">
                              <xsl:value-of select="InstallLogfile" />
                            </xsl:attribute>
                            <xsl:value-of select="SuccessfulInstall" />
                          </a>
                        </b>
                      </xsl:when>
                    </xsl:choose>
                    <xsl:text> </xsl:text>
                    <xsl:choose>
                      <xsl:when test="string-length(UnInstallLogfile) &gt; 1">
                        <xsl:text>&#187; uninstall: </xsl:text>
                        <b>
                          <a>
                            <xsl:attribute name="href">
                              <xsl:value-of select="UnInstallLogfile" />
                            </xsl:attribute>
                            <xsl:value-of select="SuccessfulUnInstall" />
                          </a>
                        </b>
                      </xsl:when>
                    </xsl:choose>
                    <xsl:choose>
                      <xsl:when test="string-length(LastError) &gt; 1">
                        <span class="failure">
                          &#187; <xsl:value-of select="LastError" />
                        </span>
                      </xsl:when>
                    </xsl:choose>
                    <span>
                      &#187; duration: <xsl:value-of select="DurationString" />
                    </span>
                    <xsl:for-each select="copyfiles/copyfile[IncludeInResults='True'] | copyfiles/copyfile[string-length(LastError) &gt; 1]">
                      &#187;
                      <xsl:choose>
                        <xsl:when test="string-length(LastError) &gt; 1">
                          <xsl:value-of select="Name" />
                          <span class="failure">
                            : <xsl:value-of select="LastError" />
                          </span>
                        </xsl:when>
                        <xsl:otherwise>
                          <xsl:choose>
                            <xsl:when test="string-length(DestFilename) &gt; 1">
                              <a>
                                <xsl:attribute name="href">
                                  <xsl:value-of select="DestFilename" />
                                </xsl:attribute>
                                <xsl:value-of select="Name" />
                              </a>
                            </xsl:when>
                            <xsl:otherwise>
                              <xsl:value-of select="Name" />
                            </xsl:otherwise>
                          </xsl:choose>
                        </xsl:otherwise>
                      </xsl:choose>
                    </xsl:for-each>
                    <xsl:for-each select="tasks/task">
                      <xsl:choose>
                        <xsl:when test="string-length(LastError) &gt; 1">
                          <span class="failure">
                            &#187;
                            <xsl:value-of select="Name" />
                            [<xsl:value-of select="CmdLine" />]
                            : <xsl:value-of select="LastError" />
                          </span>
                        </xsl:when>
                        <xsl:otherwise>
                        </xsl:otherwise>
                      </xsl:choose>
                    </xsl:for-each>
                    <xsl:for-each select="dependencies/virtualmachine">
                      <xsl:choose>
                        <xsl:when test="string-length(LastError) &gt; 1">
                          <span class="failure">
                            &#187;
                            <xsl:value-of select="Name" />
                            [<xsl:value-of select="Snapshot" />]
                            : <xsl:value-of select="LastError" />
                          </span>
                        </xsl:when>
                        <xsl:otherwise>
                          <span class="success">
                            &#187; powered:
                            <xsl:value-of select="Name" />
                            [<xsl:value-of select="Snapshot" />]
                          </span>
                        </xsl:otherwise>
                      </xsl:choose>
                    </xsl:for-each>
                  </td>
                </tr>
                <!-- include additional file data -->
                <xsl:for-each select="copyfiles/copyfile">
                  <xsl:choose>
                    <xsl:when test="string-length(Data) &gt; 1">
                      <tr>
                        <td colspan="4" class="data">
                          <xsl:value-of select="Data" disable-output-escaping="yes" />
                        </td>
                      </tr>
                    </xsl:when>
                  </xsl:choose>
                </xsl:for-each>
              </xsl:for-each>
            </table>
          </td>
        </tr>
      </xsl:for-each>      
    </table>
  </body>
</html>
