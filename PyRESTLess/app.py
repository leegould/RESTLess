from PyQt5 import QtCore, QtWidgets, QtGui
import sys
import requests
import pyrestless

class RestApp(QtWidgets.QMainWindow, pyrestless.Ui_MainWindowRESTLess):
    def __init__(self):
        QtWidgets.QMainWindow.__init__(self)
        self.setupUi(self)
        self.initUI()

    def initUI(self):
        self.lineEditUrl.setText("http://www.myapi.com")
        self.pushButtonSend.clicked.connect(self.btn_send_clicked)

    def btn_send_clicked(self):
        r = requests.get(self.lineEditUrl.text())
        self.textEditResults.setText(str(r.status_code) + ":" + r.text)

def main():
    app = QtWidgets.QApplication(sys.argv)
    restapp = RestApp()
    restapp.show()
    sys.exit(app.exec_())

if __name__ == '__main__':
    main()