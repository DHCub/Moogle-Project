if [ $# -lt 1 ]; then
	echo "Script takes one of the following arguments:"
	echo "* 'run': Runs the project"
	echo "* 'report': Generates Reporte.pdf in ../informe"
	echo "* 'slides': Generates Presentación.pdf in ../presentación"
	echo "* 'show_report <pdf viewer>': Opens ../informe/Reporte.pdf (generates it first if non-existent) with specified pdf viewer or default one if it wasn't specified" 
	echo "* 'show_slides <pdf viewer>': Opens ../presentación/Presentación.pdf (generates it first if non-existent) with specified pdf viewer or default one if it wasn't specified" 
	echo "* 'clean': Cleans unnecessary files generated upon latex compilation in ../informe and ../presentación"

elif [ $1 == "run" ]; then
	cd ..
	make dev

elif [ $1 == "report" ]; then
	cd ../informe
	pdflatex Reporte.tex

elif [ $1 == "slides" ]; then
	cd ../presentación
	pdflatex Presentación.tex
	pdflatex Presentación.tex

elif [ $1 == "show_slides" ]; then
	cd ../presentación

	if ! test -f "Presentación.pdf"; then
		cd ../script
		./proyecto.sh slides
		cd ../presentación; fi

	if [ $# -gt 1 ]; then
		$2 Presentación.pdf
	else open Presentación.pdf; fi

elif [ $1 == "show_report" ]; then
	cd ../informe

	if ! test -f "Reporte.pdf";then
		cd ../script
		./proyecto.sh report
		cd ../informe; fi

	if [ $# -gt 1 ]; then
		$2 Reporte.pdf
	else open Reporte.pdf; fi

elif [ $1 == "clean" ]; then
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
