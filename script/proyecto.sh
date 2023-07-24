# this line is added so the script can be run from any folder in the system, not only the location of the script in the project
ABS="$(dirname "$(readlink -f -- "$0")")"
# "--" added so file names like -name/ won't break the command, readlink to resolve symlinks

cd "$ABS"

function check_file()
{
	if ! test -f $1; then 
		ERROR_FOUND=1;
		echo "Missing File: $1"
	fi
}

function Generate_Presentation()
{
	ERROR_FOUND=0;
	check_file "asterisk1.png"
	check_file "asterisk2.png"
	check_file "correction.png"
	check_file "exclude1.png"
	check_file "exclude2.png"
	check_file "include1.png"
	check_file "include2.png"
	check_file "sample_search.png"
	check_file "snippet.png"
	check_file "Presentación.tex"

	if [ $ERROR_FOUND -eq 1 ];then
		echo "Cannot compile LaTeX presentation because of missing files"
	else 
		pdflatex Presentación.tex
		pdflatex Presentación.tex
	fi;
}

function Generate_Report()
{
	ERROR_FOUND=0;
	check_file "Reporte.tex"

	if [ $ERROR_FOUND -eq 1 ];then
		echo "Cannot compile LaTeX report because of missing files"
	else 
		pdflatex Reporte.tex
	fi;
}

if [ $# -lt 1 ]; then
	echo "Script takes one of the following arguments:"
	echo "* 'run': Runs the project"
	echo "* 'report': Generates Report pdf"
	echo "* 'slides': Generates Presentation pdf"
	echo "* 'show_report <pdf viewer>': Opens Report pdf (generates it first if non-existent) with specified pdf viewer or default one if it wasn't specified" 
	echo "* 'show_slides <pdf viewer>': Opens Presentation pdf (generates it first if non-existent) with specified pdf viewer or default one if it wasn't specified" 
	echo "* 'clean': Cleans unnecessary files generated upon latex compilation of Report and Presentation pdfs"

elif [ "$1" = "run" ]; then
	cd ..
	dotnet watch run --project MoogleServer

elif [ "$1" = "report" ]; then
	cd ../informe
	Generate_Report

elif [ "$1" = "slides" ]; then
	cd ../presentación
	Generate_Presentation

elif [ "$1" = "show_slides" ]; then
	cd ../presentación

	if ! test -f "Presentación.pdf"; then
		Generate_Presentation;fi

	if test -f "Presentación.pdf";then
		if [ $# -gt 1 ]; then
			"$2" Presentación.pdf
		else open Presentación.pdf; fi
	fi

elif [ "$1" = "show_report" ]; then
	cd ../informe

	if ! test -f "Reporte.pdf"; then
		Generate_Report;fi
	
	if test -f "Reporte.pdf"; then
		if [ $# -gt 1 ]; then
			"$2" Reporte.pdf
		else open Reporte.pdf; fi
	fi

elif [ "$1" = "clean" ]; then
	cd ../presentación
	if test -f "Presentación.aux"; then rm "Presentación.aux";fi
	if test -f "Presentación.log"; then rm "Presentación.log";fi
	if test -f "Presentación.nav"; then rm "Presentación.nav";fi
	if test -f "Presentación.out"; then rm "Presentación.out";fi
	if test -f "Presentación.snm"; then rm "Presentación.snm";fi
	if test -f "Presentación.toc"; then rm "Presentación.toc";fi
	if test -f "Presentación.pdf"; then rm "Presentación.pdf";fi

	cd ../informe
	if test -f "Reporte.aux"; then rm "Reporte.aux";fi
	if test -f "Reporte.log"; then rm "Reporte.log";fi
	if test -f "Reporte.pdf"; then rm "Reporte.pdf";fi
	if test -f "Reporte.toc"; then rm "Reporte.toc";fi
else
	echo "! ERROR: Unrecognized Command"
fi
