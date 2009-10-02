
TEXFILES = $(wildcard *.tex)
BIBFILES = 
# derived files: 'make derived' forcibly generates these
OTHERFILES = figures/probe-output.tex figures/current-output.tex figures/helloworld.cs.tex

SRCS = $(TEXFILES) $(BIBFILES) $(OTHERFILES) revdate.tex

TARGET = manual
LATEX = latex
LOPTS = -output-format=pdf

# all: $(TARGET).pdf $(TARGET).html.tgz TAGS
all: $(TARGET).pdf TAGS

.PHONY: derived
derived: /bin/rm -f $(OTHERFILES)

figures/probe-output.tex:
	echo '\begin{verbatim}' > $@
	curl 'http://agent.mtconnect.org/LinuxCNC/probe' | sed 's/^    //' | cat -bn >> $@
	echo '\end{verbatim}' >> $@

figures/current-output.tex:
	echo '\begin{verbatim}' > $@
	curl 'http://agent.mtconnect.org/LinuxCNC/current' | cat -bn | sed 's/^    //' >> $@
	echo '\end{verbatim}' >> $@

figures/helloworld.cs.tex: ../dotnet20/MTConnect/SimpleBrowser/Program.cs
	echo '\begin{verbatim}' > $@
	cat -bn $< | sed 's/^    //' >> $@
	echo '\end{verbatim}' >> $@

figures/helloworld2.cs.tex: ../dotnet20/MTConnect/SimpleBrowser/Program.cs
	echo '\begin{verbatim}' > $@
	cat -bn $< | sed 's/^    //' >> $@
	echo '\end{verbatim}' >> $@

$(TARGET).html.tgz: html
	tar -zcf $@ $(TARGET)

.PHONY: revdate.tex
revdate.tex:
	svn info | sed -ne 's/^Last Changed Date[^(]*//p' > $@

.PHONY: html
html:  $(TARGET).tex $(SRCS) $(TARGET).pdf
	latex2html -split 0 $<
	/bin/rm -f $(TARGET)/*.bak $(TARGET)/*.pl $(TARGET)/*.aux $(TARGET)/*.bbl $(TARGET)/*.log $(TARGET)/WARNINGS

TAGS: $(TEXFILES) $(BIBFILES)
	etags $^

# $(TARGET).pdf: $(TARGET).dvi
# 	dvipdf $<
# 	open $@

$(TARGET).pdf: $(SRCS)
	$(LATEX) $(LOPTS) $(TARGET)
	$(LATEX) $(LOPTS) $(TARGET)

clean:
	rm -f *.aux *.log *~ *.bbl *.dvi *.toc *.pdf

veryclean: clean
	rm -rf $(TARGET)/  $(TARGET).html.tgz  

