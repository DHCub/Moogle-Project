# this line is added so the script can be run from any folder in the system, not only the location of the script in the project
ABS="$(dirname "$(readlink -f -- "$0")")"
# "--" added so file names like -name/ won't break the command, readlink to resolve symlinks

cd "$ABS"

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
	pdflatex Reporte.tex

elif [ "$1" = "slides" ]; then
	cd ../presentación
	pdflatex Presentación.tex
	pdflatex Presentación.tex

elif [ "$1" = "show_slides" ]; then
	cd ../presentación

	if ! test -f "Presentación.pdf"; then
		cd ../script
		./proyecto.sh slides
		cd ../presentación; fi

	if [ $# -gt 1 ]; then
		"$2" Presentación.pdf
	else open Presentación.pdf; fi

elif [ "$1" = "show_report" ]; then
	cd ../informe

	if ! test -f "Reporte.pdf";then
		cd ../script
		./proyecto.sh report
		cd ../informe; fi

	if [ $# -gt 1 ]; then
		"$2" Reporte.pdf
	else open Reporte.pdf; fi

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
