﻿<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0"
                xmlns:wix="http://schemas.microsoft.com/wix/2006/wi">
    <xsl:output method="xml" indent="yes"/>

    <!-- copy everything verbatim -->
    <xsl:template match="@*|node()">
        <xsl:copy>
            <xsl:apply-templates select="@*|node()" />
        </xsl:copy>
    </xsl:template>

    <!--Match and ignore .exe files-->
    <xsl:key name="exe-search" match="wix:Component[contains(wix:File/@Source, 'WB.Services.Export.Host.exe')]" use="@Id"/>
    <xsl:template match="wix:Component[key('exe-search', @Id)]"/>
    <xsl:template match="wix:ComponentRef[key('exe-search', @Id)]"/>
</xsl:stylesheet>
